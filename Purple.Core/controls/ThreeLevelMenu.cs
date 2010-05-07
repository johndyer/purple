using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Purple.Core.Controls
{
	[DefaultProperty("Text")]
	[ToolboxData("<{0}:MenuControl runat=server></{0}:MenuControl>")]
	public class ThreeLevelMenu : Control
	{
		[Bindable(true)]
		[Category("Appearance")]	
		[DefaultValue("selected")]
		[Localizable(true)]
		public string SelectedCssClass
		{
			get
			{
				String s = (String)ViewState["SelectedCssClass"];
				return ((s == null) ? String.Empty : s);
			}

			set
			{
				ViewState["SelectedCssClass"] = value;
			}
		}

		protected override void RenderContents(HtmlTextWriter output)
		{
			Webpage webpage = ((PurplePage)this.Page).Webpage;


			Webpage parent = null;
			switch (webpage.MenuType)
			{
				case MenuType.Parent:
					parent = webpage;
					break;
				case MenuType.MenuItem:
					parent = webpage.Parent;
					break;
				case MenuType.SubMenuItem:
					parent = webpage.Parent.Parent;
					break;
			}

			output.Write("<ul>");
			output.Write("<li><a href=\"" + parent.Url + "\">" + parent.MenuTitle + "</a>");

			output.Write("<ul>"); // list for menu items

			foreach (Webpage menuItem in parent.ChildrenInMenu)
			{

				output.Write("<li" + ((webpage.WebpageID == menuItem.WebpageID) ? " class=\"" + this.SelectedCssClass : "") + "\"><a href=\"" + menuItem.Url + "\">" + menuItem.MenuTitle + "</a>\n");

				// if the current page is the menuItem
				// or if the current page is a subMenuItem and it's parent is the current menuItem
				if (webpage.WebpageID == menuItem.WebpageID || webpage.ParentID == menuItem.ParentID)
				{
					output.Write("<ul>\n");

					foreach (Webpage subMenuItem in menuItem.ChildrenInMenu)
					{
						output.Write("<li" + ((webpage.WebpageID == subMenuItem.WebpageID) ? " class=\"" + this.SelectedCssClass : "") + "\"><a href=\"" + menuItem.Url + "\">" + menuItem.MenuTitle + "</a></li>\n");
					}
					output.Write("</ul>\n");
				}

				output.Write("</li>");
			}

			output.Write("</ul>"); // list for menu items

			output.Write("</li>"); // parent li
			output.Write("</ul>"); // main list
			//output.Write();
			
		}
	}
}
