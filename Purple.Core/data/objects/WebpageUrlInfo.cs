using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Purple.Core
{
    public class WebpageUrlInfo
    {
        public Guid WebpageID { get; set; }
		public Guid ParentID { get; set; }
        public string Url { get; set; }
		public string Title { get; set; }
		public string MenuTitle { get; set; }
    }
}
