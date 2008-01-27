#region License

/* Copyright (C) 2007 Cristian Libardo
 *
 * This is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation; either version 2.1 of
 * the License, or (at your option) any later version.
 */

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using N2.Collections;
using N2.Details;
using N2.Persistence.Finder;
using NHibernate.Expression;

namespace N2.Persistence.NH
{
	/// <summary>
	/// A wrapper for NHibernate persistence functionality.
	/// </summary>
	public class DefaultPersister : IPersister
	{
		#region Private Fields

		private readonly IRepository<int, ContentItem> itemRepository;
		private readonly INHRepository<int, LinkDetail> linkRepository;
		private readonly IItemFinder finder;
		private bool autoFlush = false;

		#endregion

		#region Constructor & Initialize

		/// <summary>Creates a new instance of the DefaultPersistenceManager.</summary>
		public DefaultPersister(IRepository<int, ContentItem> itemRepository, INHRepository<int, LinkDetail> linkRepository,
		                        IItemFinder finder)
		{
			this.itemRepository = itemRepository;
			this.linkRepository = linkRepository;
			this.finder = finder;

			Debug.WriteLine("DefaultPersistenceManager: ctor");
		}

		#endregion

		#region Properties

		public bool AutoFlush
		{
			get { return autoFlush; }
			set { autoFlush = value; }
		}

		#endregion

		#region Load, Save, & Delete Methods

		/// <summary>Gets an item by id</summary>
		/// <param name="id">The id of the item to load</param>
		/// <returns>The item if one with a matching id was found, otherwise null.</returns>
		public virtual ContentItem Get(int id)
		{
			ContentItem item = itemRepository.Get(id);
			if (ItemLoaded != null)
				ItemLoaded.Invoke(this, new ItemEventArgs(item));
			return item;
		}

		/// <summary>Gets an item by id</summary>
		/// <typeparam name="T">The type of object that is expected</typeparam>
		/// <param name="id">The id of the item to load</param>
		/// <returns>The item if one with a matching id was found, otherwise null.</returns>
		public virtual T Get<T>(int id) where T : ContentItem
		{
			T item = itemRepository.Get<T>(id);
			if (ItemLoaded != null)
				ItemLoaded.Invoke(this, new ItemEventArgs(item));
			return item;
		}

		/// <summary>Saves or updates an item storing it in database</summary>
		/// <param name="unsavedItem">Item to save</param>
		public virtual void Save(ContentItem unsavedItem)
		{
			CancellableItemEventArgs args = new CancellableItemEventArgs(unsavedItem);
			OnSaving(args);

			if (!args.Cancel)
			{
				Trace.TraceInformation("NHPersistenceManager.Save " + unsavedItem);
				using (ITransaction transaction = itemRepository.BeginTransaction())
				{
					if (unsavedItem.VersionOf == null)
						unsavedItem.Updated = DateTime.Now;
					if (unsavedItem.Name.Length == 0)
						unsavedItem.Name = null;

					itemRepository.SaveOrUpdate(unsavedItem);

					unsavedItem.AddTo(unsavedItem.Parent);

					EnsureSortOrder(unsavedItem);

					if (AutoFlush)
						itemRepository.Flush();
					transaction.Commit();
				}
				OnSaved(new ItemEventArgs(unsavedItem));
			}
			else
				Debug.WriteLine("NHPersistenceManager.Save cancelled " + unsavedItem);
		}

		private static void EnsureSortOrder(ContentItem unsavedItem)
		{
			if (unsavedItem.Parent != null)
				Utility.UpdateSortOrder(unsavedItem.Parent.Children);
		}

		#region Save Helper Methods

		/// <summary>Invokes saving events and checks whether the operation should be performed.</summary>
		protected virtual void OnSaving(CancellableItemEventArgs args)
		{
			if (ItemSaving != null && args.AffectedItem.VersionOf == null)
				ItemSaving.Invoke(this, args);
		}

		/// <summary>Invokes the saved event.</summary>
		protected virtual void OnSaved(ItemEventArgs args)
		{
			if (ItemSaved != null && args.AffectedItem.VersionOf == null)
				ItemSaved.Invoke(this, args);
		}

		#endregion

		/// <summary>Deletes an item an all sub-items</summary>
		/// <param name="itemNoMore">The item to delete</param>
		public void Delete(ContentItem itemNoMore)
		{
			CancellableItemEventArgs args = new CancellableItemEventArgs(itemNoMore);
			OnDeleting(args);
			if (!args.Cancel)
			{
				Trace.TraceInformation("NHPersistenceManager.Delete" + itemNoMore);

				using (ITransaction transaction = itemRepository.BeginTransaction())
				{
					DeleteRecursive(itemNoMore);

					if (AutoFlush)
						itemRepository.Flush();
					transaction.Commit();
				}
				OnDeleted(new ItemEventArgs(itemNoMore));
			}
			else
				Debug.WriteLine("NHPersistenceManager.Delete cancelled " + itemNoMore);
		}

		#region Delete Helper Methods

		private void DeleteRecursive(ContentItem itemNoMore)
		{
			DeletePreviousVersions(itemNoMore);

			while (itemNoMore.Children.Count > 0)
				DeleteRecursive(itemNoMore.Children[0]);

			itemNoMore.AddTo(null);

			DeleteInboundLinks(itemNoMore);

			itemRepository.Delete(itemNoMore);
		}

		private void DeletePreviousVersions(ContentItem itemNoMore)
		{
			foreach (ContentItem previousVersion in finder.Where.VersionOf.Eq(itemNoMore).Select())
			{
				itemRepository.Delete(previousVersion);
				if (AutoFlush)
					itemRepository.Flush();
			}
		}

		private void DeleteInboundLinks(ContentItem itemNoMore)
		{
			foreach (LinkDetail detail in linkRepository.FindAll(Expression.Eq("LinkedItem", itemNoMore)))
			{
				if (detail.EnclosingCollection != null)
					detail.EnclosingCollection.Remove(detail);
				detail.EnclosingItem.Details.Remove(detail.Name);
				linkRepository.Delete(detail);
			}
		}

		/// <summary>Invokes deleting events and checks whether the operation should be performed.</summary>
		protected virtual void OnDeleting(CancellableItemEventArgs args)
		{
			if (ItemDeleting != null && args.AffectedItem.VersionOf == null)
				ItemDeleting.Invoke(this, args);
		}

		/// <summary>Invokes deleted events.</summary>
		protected virtual void OnDeleted(ItemEventArgs args)
		{
			if (ItemDeleted != null && args.AffectedItem.VersionOf == null)
				ItemDeleted.Invoke(this, args);
		}

		#endregion

		#endregion

		#region Move & Copy Methods

		/// <summary>Move an item to a destination</summary>
		/// <param name="source">The item to move</param>
		/// <param name="destination">The destination below which to place the item</param>
		public virtual void Move(ContentItem source, ContentItem destination)
		{
			CancellableDestinationEventArgs args = new CancellableDestinationEventArgs(source, destination);
			OnMoving(args);

			if (!args.Cancel)
			{
				using (ITransaction transaction = itemRepository.BeginTransaction())
				{
					source.AddTo(destination);
					itemRepository.Save(source);
					if (AutoFlush)
						itemRepository.Flush();

					transaction.Commit();
				}
				OnMoved(new DestinationEventArgs(source, destination));
			}
		}

		/// <summary>Persists changes.</summary>
		public void Flush()
		{
			itemRepository.Flush();
		}

		#region Move Helper Methods

		private void OnMoving(CancellableDestinationEventArgs args)
		{
			if (ItemMoving != null)
				ItemMoving.Invoke(this, args);
			if (args.Cancel)
				Debug.WriteLine("NHPersistenceManager.Move cancelled " + args.AffectedItem);
		}

		private void OnMoved(DestinationEventArgs args)
		{
			if (ItemMoved != null)
				ItemMoved.Invoke(this, args);
		}

		#endregion

		/// <summary>Copies an item and all sub-items to a destination</summary>
		/// <param name="source">The item to copy</param>
		/// <param name="destination">The destination below which to place the copied item</param>
		/// <returns>The copied item</returns>
		public virtual ContentItem Copy(ContentItem source, ContentItem destination)
		{
			return Copy(source, destination, true);
		}

		/// <summary>Copies an item and all sub-items to a destination</summary>
		/// <param name="source">The item to copy</param>
		/// <param name="destination">The destination below which to place the copied item</param>
		/// <param name="includeChildren">Whether child items should be copied as well.</param>
		/// <returns>The copied item</returns>
		public virtual ContentItem Copy(ContentItem source, ContentItem destination, bool includeChildren)
		{
			if (ShouldCopy(source, destination))
			{
				Trace.TraceInformation("NHPersistenceManager.Copy " + source);
				ContentItem cloned = source.Clone(includeChildren);

				cloned.Parent = destination;
				Save(cloned);

				if (ItemCopied != null)
					ItemCopied.Invoke(this, new DestinationEventArgs(cloned, destination));

				return cloned;
			}
			else
			{
				Debug.WriteLine("NHPersistenceManager.Copy cancel " + source);
				return null;
			}
		}

		/// <summary>Invokes copying events and checks whether the copy operation should be performed.</summary>
		/// <returns>True if the copying should proceed.</returns>
		private bool ShouldCopy(ContentItem source, ContentItem destination)
		{
			CancellableDestinationEventArgs cdea = new CancellableDestinationEventArgs(source, destination);
			if (ItemCopying != null)
				ItemCopying.Invoke(this, cdea);

			return !cdea.Cancel;
		}

		#endregion

		#region List Methods

		/// <summary>Gets child itms recursively.</summary>
		/// <param name="item">The item whose offspring to get. The item itself is not returned.</param>
		/// <param name="preFilters">Filters to apply before recursing into the next level.</param>
		/// <returns>A list of children and the rest of the offspring of an item.</returns>
		/// <remarks>This method does not consider authorization. The items are retrieved by recursively querying for items child items.</remarks>
		public virtual IList<ContentItem> GetChildrenRecursive(ContentItem item, params ItemFilter[] preFilters)
		{
			ItemList items = new ItemList(item.Children, preFilters);
			int count = items.Count;
			for (int i = 0; i < count; i++)
				items.AddRange(GetChildrenRecursive(items[i]));
			return items;
		}

		#endregion

		#region IPersistenceEventSource

		/// <summary>Occurs before an item is saved</summary>
		public event EventHandler<CancellableItemEventArgs> ItemSaving;

		/// <summary>Occurs when an item has been saved</summary>
		public event EventHandler<ItemEventArgs> ItemSaved;

		/// <summary>Occurs before an item is deleted</summary>
		public event EventHandler<CancellableItemEventArgs> ItemDeleting;

		/// <summary>Occurs when an item has been deleted</summary>
		public event EventHandler<ItemEventArgs> ItemDeleted;

		/// <summary>Occurs before an item is moved</summary>
		public event EventHandler<CancellableDestinationEventArgs> ItemMoving;

		/// <summary>Occurs when an item has been moved</summary>
		public event EventHandler<DestinationEventArgs> ItemMoved;

		/// <summary>Occurs before an item is copied</summary>
		public event EventHandler<CancellableDestinationEventArgs> ItemCopying;

		/// <summary>Occurs when an item has been copied</summary>
		public event EventHandler<DestinationEventArgs> ItemCopied;

		/// <summary>Occurs when an item is loaded</summary>
		public event EventHandler<ItemEventArgs> ItemLoaded;

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			itemRepository.Dispose();
		}

		#endregion
	}
}