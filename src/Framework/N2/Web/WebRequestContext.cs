using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Collections;
using System.Security.Principal;
using System.Diagnostics;
using System.Web.Hosting;

namespace N2.Web
{
    /// <summary>
    /// A request context class that interacts with HttpContext.Current.
    /// </summary>
    public class WebRequestContext : IWebContext, IDisposable
    {
        /// <summary>Provides access to HttpContext.Current.</summary>
        protected virtual HttpContext CurrentHttpContext
        {
            get
            {
                if (HttpContext.Current == null)
                    throw new N2Exception("Tried to retrieve HttpContext.Current but it's null. This may happen when working outside a request or when doing stuff after the context has been recycled.");
                return HttpContext.Current;
            }
        }

        public bool IsWeb
        {
            get { return true; }
        }

        /// <summary>Gets a dictionary of request scoped items.</summary>
        public IDictionary RequestItems
        {
            get { return CurrentHttpContext.Items; }
        }

        /// <summary>A page instance stored in the request context.</summary>
        public ContentItem CurrentPage
        {
            get { return RequestItems["CurrentPage"] as ContentItem; }
            set { RequestItems["CurrentPage"] = value; }
		}

		/// <summary>The template used to serve this request.</summary>
		public PathData CurrentPath
		{
			get { return RequestItems["CurrentTemplate"] as PathData ?? PathData.Empty; }
			set
			{
				RequestItems["CurrentTemplate"] = value;
				if (value != null)
					CurrentPage = value.CurrentPage;
				else
					CurrentPage = null;
			}
		}

		/// <summary>Specifies whether the UrlAuthorizationModule should skip authorization for the current request.</summary>
		public bool SkipAuthorization
		{
			get { return CurrentHttpContext.SkipAuthorization; }
			set { CurrentHttpContext.SkipAuthorization = value; }
		}

        /// <summary>The handler associated with the current request.</summary>
        public IHttpHandler Handler
        {
            get { return CurrentHttpContext.Handler; }
        }

        /// <summary>The current request object.</summary>
        public HttpRequest Request
        {
            get { return CurrentHttpContext.Request; }
        }

        /// <summary>The physical path on disk to the requested resource.</summary>
        public virtual string PhysicalPath
        {
            get { return Request.PhysicalPath; }
        }

		/// <summary>The host part of the requested url, e.g. http://n2cms.com/path/to/a/page.aspx?some=query.</summary>
        public Url Url
        {
            get { return new Url(Request.Url.Scheme, Request.Url.Authority, Request.RawUrl); }
        }

        /// <summary>The current request object.</summary>
        public HttpResponse Response
        {
            get { return CurrentHttpContext.Response; }
        }

        /// <summary>Gets the current user in the web execution context.</summary>
        public IPrincipal User
        {
            get { return CurrentHttpContext.User; }
        }

        /// <summary>Converts a virtual url to an absolute url.</summary>
        /// <param name="virtualPath">The virtual url to make absolute.</param>
        /// <returns>The absolute url.</returns>
        public virtual string ToAbsolute(string virtualPath)
        {
            return N2.Web.Url.ToAbsolute(virtualPath);
        }

        /// <summary>Converts an absolute url to an app relative url.</summary>
        /// <param name="virtualPath">The absolute url to convert.</param>
        /// <returns>An app relative url.</returns>
        public virtual string ToAppRelative(string virtualPath)
        {
            if (virtualPath != null && virtualPath.StartsWith(Url.ApplicationPath, System.StringComparison.InvariantCultureIgnoreCase))
                return "~/" + virtualPath.Substring(Url.ApplicationPath.Length);
            return virtualPath;
        }

        /// <summary>Maps a virtual path to a physical disk path.</summary>
        /// <param name="path">The path to map. E.g. "~/bin"</param>
        /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
        public string MapPath(string path)
        {
			return HostingEnvironment.MapPath(path);
        }

		/// <summary>Assigns a rewrite path.</summary>
		/// <param name="path">The path to the template that will handle the request.</param>
		public void RewritePath(string path)
		{
			Debug.WriteLine("Rewriting '" + Url.LocalUrl + "' to '" + path + "'");
			CurrentHttpContext.RewritePath(path, false);
		}

		/// <summary>Assigns a rewrite path.</summary>
		/// <param name="path">The path to the template that will handle the request.</param>
		/// <param name="queryString">The query string to rewrite to.</param>
		public void RewritePath(string path, string queryString)
		{
			Debug.WriteLine("Rewriting '" + Url.LocalUrl + "' to '" + path + "'");
			CurrentHttpContext.RewritePath(path, "", queryString);
		}

        public void TransferRequest(string path)
        {
            string url = Url.Parse(path).AppendQuery("postback", Url.LocalUrl);
            CurrentHttpContext.Server.TransferRequest(url, true);
        }

		/// <summary>Calls into HttpContext.ClearError().</summary>
		public void ClearError()
		{
			CurrentHttpContext.ClearError();
		}

        /// <summary>Disposes request items that needs disposing. This method should be called at the end of each request.</summary>
		public virtual void Close()
        {
            object[] keys = new object[RequestItems.Keys.Count];
            RequestItems.Keys.CopyTo(keys, 0);

            foreach (object key in keys)
            {
                IClosable value = RequestItems[key] as IClosable;
                if (value != null)
                {
                    value.Dispose();
                }
            }
        }

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			Close();
		}

		#endregion
	}
}
