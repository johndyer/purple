using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Purple.Core
{
    public class PurpleRoutes
    {

		private static RouteCollection _routes;
		private static readonly string _purpleCmsPagesRouteName = "PurpleCmsPagesRoute";
		private static readonly string _purpleRedirectsRouteName = "PurpleUrlRedirectsRoute";
		
        public static void AddRoutes(RouteCollection routes) {

			_routes = routes;

			ReloadRoutes();
		}

		private static void ReloadRoutes() {
			// load custom routes
			List<CustomRouteEntry> customRouteEntries = CustomRouteEntries.GetCustomRouteEntries();


			/*
			// remove all custom routes
			foreach (CustomRouteEntry customRouteEntry in customRouteEntries)
			{
				if (_routes[customRouteEntry.Name] != null)
					_routes.Remove(_routes[customRouteEntry.Name]);
			}

			// remove default route
			if (_routes[_purpleRouteName] != null)
				_routes.Remove(_routes[_purpleRouteName]);
			*/

            // ignores
			_routes.Ignore("cms/{*url}");


			// add custom routes
			foreach (CustomRouteEntry customRouteEntry in customRouteEntries)
			{
				Type routeType = null;
				PurpleCmsHandlerBase handler = null;
				
				routeType = Type.GetType(customRouteEntry.Type);
				handler = Activator.CreateInstance(routeType) as PurpleCmsHandlerBase;

				_routes.Add(customRouteEntry.Name, new Route
				(
					customRouteEntry.Url,
					handler
				));

			}

			// add redirects
			_routes.Add(_purpleRedirectsRouteName, new Route
			 (
				"{*url}",
				new RouteValueDictionary { { "url", "invalid" } },
				new RouteValueDictionary { { "url", new PurpleUrlRedirectConstraint() } },
				new PurpleUrlRedirectHandler()
			 ));


			// add the main handler for database pages
			_routes.Add(_purpleCmsPagesRouteName, new Route
			 (
				"{*url}",
				new RouteValueDictionary{{"url","site-root"}},
				new RouteValueDictionary { { "url", new PurplePageConstraint()} },
				new PurplePageRouteHandler()
			 ));
        }
    }


	public class PurpleUrlRedirectConstraint : IRouteConstraint
	{
		public PurpleUrlRedirectConstraint()
		{
		}


		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			string url = (values[parameterName] as string).ToLower();
			
			return UrlRedirects.ActiveUrlRedirects.Any(u => u.FromUrl == url || u.FromUrl == url + "/");
		}
	}


    public class PurplePageConstraint : IRouteConstraint
    {
        public PurplePageConstraint()
        {            
        }


        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            string url = (values[parameterName] as string).ToLower();
			return Webpages.ValidateRouteUrl(url); 
        }
    }
}
