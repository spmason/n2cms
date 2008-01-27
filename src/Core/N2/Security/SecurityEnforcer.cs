using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core;
using System.Diagnostics;
using System.Security.Principal;

namespace N2.Security
{
	public class SecurityEnforcer : ISecurityEnforcer, IStartable
	{
		private Persistence.IPersister persister;
		private ISecurityManager security;
		private Web.IUrlParser urlParser;
		private Web.IWebContext webContext;

		public SecurityEnforcer(Persistence.IPersister persister, ISecurityManager security, Web.IUrlParser urlParser, Web.IWebContext webContext)
		{
			this.webContext = webContext;
			this.persister = persister;
			this.security = security;
			this.urlParser = urlParser;
		}

		#region Event Handlers
		private void ItemSavingEventHandler(object sender, N2.Persistence.CancellableItemEventArgs e)
		{
			if (security.Enabled && security.ScopeEnabled)
				OnItemSaving(e.AffectedItem);
		}

		private void ItemMovingEvenHandler(object sender, N2.Persistence.CancellableDestinationEventArgs e)
		{
			if (security.Enabled && security.ScopeEnabled)
				OnItemMoving(e.AffectedItem, e.Destination);
		}

		private void ItemDeletingEvenHandler(object sender, N2.Persistence.CancellableItemEventArgs e)
		{
			if (security.Enabled && security.ScopeEnabled)
				OnItemDeleting(e.AffectedItem);
		}

		private void ItemCopyingEvenHandler(object sender, N2.Persistence.CancellableDestinationEventArgs e)
		{
			if (security.Enabled && security.ScopeEnabled)
				OnItemCopying(e.AffectedItem, e.Destination);
		}
		#endregion

		#region Methods

		/// <summary>Checks that the current user is authorized to access the current item.</summary>
		/// <param name="context">The context of the request.</param>
		public virtual void AuthorizeRequest(Web.IWebContext context)
		{
			string filePath = context.Request.Url.AbsolutePath.ToLower();
			string pathWithQuery = context.Request.Url.PathAndQuery;
			if (filePath.EndsWith(urlParser.DefaultExtension, StringComparison.InvariantCultureIgnoreCase)
				&& !filePath.StartsWith(context.ToAbsolute("~/edit/"), StringComparison.InvariantCultureIgnoreCase))
			{
				ContentItem item = urlParser.Parse(pathWithQuery);
				if (item != null && !security.IsAuthorized(item, context.CurrentUser))
				{
					Debug.WriteLine("Permission denied to item " + item.ID + " for user " + System.Web.UI.DataBinder.Eval(context, "CurrentUser.Identity.Name"));
					throw new N2.Security.PermissionDeniedException(item, context.CurrentUser);
				}
			}
		}

		/// <summary>Is invoked when an item is saved.</summary>
		/// <param name="item">The item that is to be saved.</param>
		protected virtual void OnItemSaving(ContentItem item)
		{
			if (!security.IsAuthorized(item, this.webContext.CurrentUser))
				throw new PermissionDeniedException(item, this.webContext.CurrentUser);
			IPrincipal user = this.webContext.CurrentUser;
			if (user != null)
				item.SavedBy = user.Identity.Name;
			else
				item.SavedBy = null;
		}

		/// <summary>Is Invoked when an item is moved.</summary>
		/// <param name="source">The item that is to be moved.</param>
		/// <param name="destination">The destination for the item.</param>
		protected virtual void OnItemMoving(ContentItem source, ContentItem destination)
		{
			if (!security.IsAuthorized(source, this.webContext.CurrentUser) || !security.IsAuthorized(destination, this.webContext.CurrentUser))
				throw new PermissionDeniedException(source, this.webContext.CurrentUser);
		}

		/// <summary>Is invoked when an item is to be deleted.</summary>
		/// <param name="item">The item to delete.</param>
		protected virtual void OnItemDeleting(ContentItem item)
		{
			IPrincipal user = webContext.CurrentUser;
			if (!security.IsAuthorized(item, user))
				throw new PermissionDeniedException(item, user);
		}

		/// <summary>Is invoked when an item is to be copied.</summary>
		/// <param name="source">The item that is to be copied.</param>
		/// <param name="destination">The destination for the copied item.</param>
		protected virtual void OnItemCopying(ContentItem source, ContentItem destination)
		{
			if (!security.IsAuthorized(source, this.webContext.CurrentUser) || !security.IsAuthorized(destination, this.webContext.CurrentUser))
				throw new PermissionDeniedException(source, this.webContext.CurrentUser);
		}
		#endregion

		#region IStartable Members

		public virtual void Start()
		{
			persister.ItemSaving += new EventHandler<N2.Persistence.CancellableItemEventArgs>(ItemSavingEventHandler);
			persister.ItemCopying += new EventHandler<N2.Persistence.CancellableDestinationEventArgs>(ItemCopyingEvenHandler);
			persister.ItemDeleting += new EventHandler<N2.Persistence.CancellableItemEventArgs>(ItemDeletingEvenHandler);
			persister.ItemMoving += new EventHandler<N2.Persistence.CancellableDestinationEventArgs>(ItemMovingEvenHandler);
		}

		public virtual void Stop()
		{
			persister.ItemSaving -= new EventHandler<N2.Persistence.CancellableItemEventArgs>(ItemSavingEventHandler);
			persister.ItemCopying -= new EventHandler<N2.Persistence.CancellableDestinationEventArgs>(ItemCopyingEvenHandler);
			persister.ItemDeleting -= new EventHandler<N2.Persistence.CancellableItemEventArgs>(ItemDeletingEvenHandler);
			persister.ItemMoving -= new EventHandler<N2.Persistence.CancellableDestinationEventArgs>(ItemMovingEvenHandler);
		}

		#endregion
	}
}