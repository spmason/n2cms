﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using N2.Persistence.Finder;
using N2.Definitions;
using N2.Persistence;
using N2.Web;
using N2.Engine;

namespace N2.Edit
{
	/// <summary>
	/// Gets and creates a container for management items.
	/// </summary>
	[Service]
	public class ContainerRepository<T> where T: ContentItem
	{
		IItemFinder finder;
		IPersister persister;
		IHost host;
		IDefinitionManager definitions;

		/// <summary>Instructs this class to navigate the content hierarchy rather than query for items.</summary>
		public bool Navigate { get; set; }

		/// <summary>Stores dependencies</summary>
		/// <param name="finder"></param>
		/// <param name="persister"></param>
		/// <param name="definitions"></param>
		public ContainerRepository(IPersister persister, IItemFinder finder, IHost host, IDefinitionManager definitions)
		{
			this.finder = finder;
			this.persister = persister;
			this.host = host;
			this.definitions = definitions;
		}

		/// <summary>Gets a container below the root page or null if no container exists.</summary>
		/// <returns></returns>
		public virtual T GetBelowRoot()
		{
			return Get(persister.Get(host.CurrentSite.RootItemID));
		}

		/// <summary>Gets a container below the root page and creates it if no container exists.</summary>
		/// <param name="setupCreatedItem"></param>
		/// <returns></returns>
		public virtual T GetOrCreateBelowRoot(Action<T> setupCreatedItem)
		{
			return GetOrCreate(persister.Get(host.CurrentSite.RootItemID), setupCreatedItem);
		}

		/// <summary>Gets a container or null if no container exists.</summary>
		/// <param name="containerContainer"></param>
		/// <returns></returns>
		public virtual T Get(ContentItem containerContainer) 
		{
			if (Navigate)
			{
				return containerContainer.Children.OfType<T>().FirstOrDefault();
			}
			else
			{
				var items = finder.Where.Parent.Eq(containerContainer)
					.And.Type.Eq(typeof(T))
					.MaxResults(1)
					.Select<T>();
				return items.Count > 0 ? items[0] : null;
			}
		}

		/// <summary>Gets or creates a container.</summary>
		/// <param name="containerContainer"></param>
		/// <param name="setupCreatedItem"></param>
		/// <returns></returns>
		public virtual T GetOrCreate(ContentItem containerContainer, Action<T> setupCreatedItem)
		{
			return Get(containerContainer) ?? Create(containerContainer, setupCreatedItem);
		}

		/// <summary>Creates a container.</summary>
		/// <param name="containerContainer"></param>
		/// <param name="setupCreatedItem"></param>
		/// <returns></returns>
		protected virtual T Create(ContentItem containerContainer, Action<T> setupCreatedItem)
		{
			var container = definitions.CreateInstance<T>(containerContainer);
			setupCreatedItem(container);
			persister.Save(container);
			return container;
		}
	}
}
