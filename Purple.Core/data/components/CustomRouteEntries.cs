using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Purple.Core
{
	public class CustomRouteEntries
	{
		public static List<CustomRouteEntry> GetCustomRouteEntries()
		{
			return PurpleDataProviderManager.Provider.GetCustomRouteEntries();
		}
	}
}
