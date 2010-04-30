using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Web;
using System.Web.Compilation;

namespace Purple.Core
{
	public class Webpages
	{
		private static List<WebpageUrlInfo> _webPageUrls = null;
		public static List<WebpageUrlInfo> WebpageUrls
		{
			get
			{
				if (_webPageUrls == null)
					ReloadPages();

				return _webPageUrls;
			}			
		}

        private static void ReloadPages()
        {
			_webPageUrls = PurpleDataProviderManager.Provider.GetWebpageUrls();
        }

		public static Webpage GetPublishedWebpage(string url)
		{
			return PurpleDataProviderManager.Provider.GetPublishedWebpage(url);
		}

		public static Webpage GetPublishedWebpage(Guid webpageID)
		{
			return PurpleDataProviderManager.Provider.GetPublishedWebpage(webpageID);
		}

		public static Guid AddWebpage(Webpage webpage)
		{
			webpage.IsPublished = true;
			webpage.RevisedDate = DateTime.Now;
			webpage.RevisedByUsername = (HttpContext.Current != null && HttpContext.Current.User.Identity.IsAuthenticated) ? HttpContext.Current.User.Identity.Name : "";

			PurpleDataProviderManager.Provider.AddWebpage(webpage);

			ReloadPages();

			return webpage.WebpageID;
		}

		public static void UpdateWebpage(Webpage webpage)
		{
			webpage.RevisedDate = DateTime.Now;
			webpage.RevisedByUsername = (HttpContext.Current != null && HttpContext.Current.User.Identity.IsAuthenticated) ? HttpContext.Current.User.Identity.Name : "";

			PurpleDataProviderManager.Provider.UpdateWebpage(webpage);

			ReloadPages();
		}

		public static List<Webpage> GetChildPages(Guid webpageID)
		{
			return PurpleDataProviderManager.Provider.GetChildWebpages(webpageID);
		}

		public static object GetWebpageRevision(Guid webpageID)
		{
			return PurpleDataProviderManager.Provider.GetWebpageRevisions(webpageID);
		}

		public static bool ValidateRouteUrl(string url)
		{
			return WebpageUrls.Any(w => w.Url == url || w.Url + "/" == url) // normal urls in DB (is this too expensive?)
					|| url == "site-root"  /* special case for home page */
					|| url == "default.aspx"; /* special case for home page */
		}
	}

}
