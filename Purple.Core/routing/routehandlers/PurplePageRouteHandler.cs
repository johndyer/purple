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
	public class PurplePageRouteHandler : PurpleCmsHandlerBase		
	{

		public override Webpage CreateWebpage(RequestContext requestContext)
		{
			string url = requestContext.RouteData.Values["url"] as string;

			
			// special redirect for site.com/default.aspx
			if (url == "default.aspx")
				HttpContext.Current.Response.RedirectPermanent("/");


			// redirects for / or no /
			string prefix = "/";
			if (PurpleSettings.TrailingSlash && !url.EndsWith("/"))
			{
				requestContext.HttpContext.Response.RedirectPermanent(prefix + url + "/" + HttpContext.Current.Request.Url.Query);
			}
			else if (!PurpleSettings.TrailingSlash && url.EndsWith("/") && url != "/")
			{
				requestContext.HttpContext.Response.RedirectPermanent(prefix + url.TrimEnd(new char[] { '/' }) + HttpContext.Current.Request.Url.Query);
			}
			

			// always check this
			if (url.EndsWith("/"))
				url= url.TrimEnd(new char[] {'/'});
			Webpage webpage = Webpages.GetPublishedWebpage(url);

			// run SSL check
			if (webpage != null && webpage.ForceSsl && !requestContext.HttpContext.Request.IsSecureConnection)
				requestContext.HttpContext.Response.Redirect(PurpleSettings.SecureUrl + url);			

			return webpage;
		}
	}
}
