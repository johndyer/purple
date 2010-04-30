using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Purple.Core
{
	public class CustomRouteEntry
	{
		public CustomRouteEntry()
		{
			Name = string.Empty;
			Type = string.Empty;
			Url = string.Empty;
		}

		public string Name { get; set; }
		public string Type { get; set; }
		public string Url { get; set; }

	}
}
