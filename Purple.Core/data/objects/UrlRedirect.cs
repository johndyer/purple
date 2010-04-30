using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Purple.Core
{
	public class UrlRedirect
	{
		public UrlRedirect() {
			RedirectID = Guid.Empty;
			Title = string.Empty;
			Description = string.Empty;
			IsActive = false;
			IsPermanent = false;
			FromUrl = string.Empty;
			ToUrl = string.Empty;
		}

		public Guid RedirectID { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public bool IsActive { get; set; }
		public bool IsPermanent { get; set; }
		public string FromUrl { get; set; }
		public string ToUrl { get; set; }
	}
}
