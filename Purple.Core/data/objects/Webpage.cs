using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Purple.Core
{

    public class Webpage : WebpageUrlInfo
	{
		public Webpage():this(false) {

		}

		internal Webpage(bool isEditable)
		{
			IsEditable = isEditable;

			Editors = string.Empty;
			RevisedByUsername = string.Empty;
			RevisedDate = DateTime.MinValue;
			ShowInMenu = false;
			MenuType = (MenuType)0;
			SortOrder = 9999;
			FullUrl = string.Empty;
			RevisionID = Guid.Empty;
			IsSiteRoot = false;
			IsPublished = false;
			ForceSsl = false;
			Filename = string.Empty;
			MasterPageFilename = string.Empty;
			MetaDescription = string.Empty;
			MetaKeywords = string.Empty;
			Areas = new List<WebpageArea>();

            CommonAreaHeader = string.Empty;
            IgnoreParentHeader = false;
            ContentExpirationDate = DateTime.MinValue;
		}

		public bool IsEditable { get; private set; }		
		public string Editors { get; set; }
		
		public string RevisedByUsername { get; set; }
		public DateTime RevisedDate { get; set; }

		public bool ShowInMenu { get; set; } // also site map
		public MenuType MenuType { get; set; } // where to appear in the menu		
		public int SortOrder { get; set; } // for menus
		public string FullUrl { get; set; } // this is where the page is hierarchically, when the Url has been overrided

		public Guid RevisionID { get; set; }
		public bool IsSiteRoot { get; set; }
		public bool IsPublished { get; set; }
		public bool ForceSsl { get; set; }

		public string Filename { get; set; }
		public string MasterPageFilename { get; set;}		
		public string MetaDescription { get; set; }
		public string MetaKeywords{ get; set; }

        /// <summary>
        /// HTML shared by all subpages
        /// </summary>
        public string CommonAreaHeader { get; set; }
        public bool IgnoreParentHeader { get; set; }
        public DateTime ContentExpirationDate { get; set; }

		public List<WebpageArea> Areas { get; private set; }		


		#region Lazy Loaded
		private Webpage _parent = null;
		public Webpage Parent
		{
			get
			{
				if (_parent == null)
					_parent = Webpages.GetPublishedWebpage(ParentID);

				return _parent;
			}
		}

		private List<Webpage> _children = null;
		public List<Webpage> Children 
		{
			get
			{
				if (_children == null)
					_children = Webpages.GetChildPages(WebpageID);

				return _children;
			}
		}

		private List<Webpage> _childrenInMenu = null;
		public List<Webpage> ChildrenInMenu
		{
			get
			{
				if (_childrenInMenu == null)
				{
					_childrenInMenu = Children;
					_childrenInMenu.RemoveAll(delegate(Webpage webpage) { return !webpage.ShowInMenu; });
				}

				return _childrenInMenu;
			}
		}
		#endregion
	}

}
