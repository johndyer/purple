using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.Routing;
using System.Web.Compilation;

namespace Purple.Core
{
	public class PurpleUrlRedirectHandler : IRouteHandler
	{
		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			string url = requestContext.RouteData.Values["url"] as string;

			UrlRedirect urlRedirect = UrlRedirects.GetUrlRedirect(url);

			if (urlRedirect.IsPermanent) {
				requestContext.HttpContext.Response.RedirectPermanent(urlRedirect.ToUrl, true);
			} else {
				requestContext.HttpContext.Response.Redirect(urlRedirect.ToUrl, true);
			}

			return null;
		}
	}
}
