using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;

namespace Purple.Core
{
    public abstract class PurpleDataProvider : ProviderBase
    {
        public abstract List<WebpageUrlInfo> GetWebpageUrls();

        public abstract Guid AddWebpage(Webpage webpage);
        public abstract void DeleteWebpage(Guid webpageID);
        public abstract void UpdateWebpage(Webpage webpage);

        public abstract Webpage GetWebpage(Guid webpageID, Guid revisionID);
        public abstract Webpage GetPublishedWebpage(Guid webpageID);
        public abstract Webpage GetPublishedWebpage(string url);

		public abstract List<Webpage> GetWebpageRevisions(Guid webpageID);
		public abstract Webpage GetParentWebpage(Guid webpageID);
		public abstract List<Webpage> GetChildWebpages(Guid webpageID);

		public abstract List<UrlRedirect> GetUrlRedirects();
		public abstract UrlRedirect GetUrlRedirect(string fromUrl);
		public abstract UrlRedirect GetUrlRedirect(Guid redirectID);
		public abstract void AddUrlRedirect(UrlRedirect urlRedirect);
		public abstract void UpdateUrlRedirect(UrlRedirect urlRedirect);
		public abstract void DeleteUrlRedirect(UrlRedirect urlRedirect);

		
		public abstract List<CustomRouteEntry> GetCustomRouteEntries();
		public abstract CustomRouteEntry GetCustomRouteEntry(string routeName);
		public abstract void AddCustomRouteEntry(CustomRouteEntry customRouteEntry);
		public abstract void UpdateCustomRouteEntry(CustomRouteEntry customRouteEntry);
		public abstract void DeleteCustomRouteEntry(string routeName);

	}
}