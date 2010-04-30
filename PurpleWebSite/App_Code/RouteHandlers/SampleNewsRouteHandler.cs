using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.Routing;
using System.Web.Compilation;
using Purple.Core;

namespace RouteHandlerSamples
{
	public class SampleNewsRouteHandler : PurpleCmsHandlerBase		
	{
		public override Webpage CreateWebpage(RequestContext requestContext)
		{
			string newsid = requestContext.RouteData.Values["newsid"] as string;


			// look up news item
			// NewsItem newsItem = NewsItems.FindNewsItem(newsid);

			// construct page for 
			Webpage webpage = new Webpage();
			webpage.Title = "Some news";

			if (string.IsNullOrEmpty(newsid))
			{
				webpage.Areas.Add(new WebpageArea() { ContentHtml = "<h2>News page</h2><p>This is what happens with nothing in the URL.</p>" });
			}
			else
			{
				webpage.Areas.Add(new WebpageArea() { ContentHtml = "<h2>News page</h2><h3>News Itesm: " + newsid + "</h3><p>Blah blah</p>" });
			}

			return webpage;
		}
	}
}
