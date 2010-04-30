using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Purple.Core
{
    public class WebpageArea
    {
		public Guid AreaID { get; set; }
		public Guid WebpageID { get; set; }
        public Guid RevisionID { get; set; }
        public int SortOrder { get; set; }

        public string ContentPlaceHolderID { get; set; }
		public string ContentHtml { get; set; }
        public string ControlName { get; set; }       
    }
}
