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
	public abstract class PurpleCmsHandlerBase : IRouteHandler
	{
		#region IRouteHandler Members

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			var page = BuildManager.CreateInstanceFromVirtualPath("~/cms/default.aspx", typeof(PurplePage)) as PurplePage;

			page.Webpage = CreateWebpage(requestContext);

			return (IHttpHandler)page;
		}

		public abstract Webpage CreateWebpage(RequestContext requestContext);

		#endregion
	}
}
