using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Purple.Core;

namespace Purple.Controls
{
	public enum MenuSource
	{
		CurrentPage,
		Root
	}

	public enum MenuDisplay
	{
		TriLevel,
		Flat,
		FlatWithParent
	}


	[ToolboxData("<{0}:MenuControl runat=server></{0}:MenuControl>")]
	public class MenuControl : WebControl
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
				return ((s == null) ? "selected" : s);
			}

			set
			{
				ViewState["SelectedCssClass"] = value;
			}
		}

		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue("CurrentPage")]
		[Localizable(true)]
		public MenuSource MenuSource
		{
			get
			{
				string s = (string)ViewState["MenuSource"];
				return (s == null) ? MenuSource.CurrentPage : (MenuSource) Enum.Parse(typeof(MenuSource), s);
			}

			set
			{
				ViewState["MenuSource"] = value.ToString();
			}
		}

		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue("TriLevel")]
		[Localizable(true)]
		public MenuDisplay MenuDisplay
		{
			get
			{
				string s = (string)ViewState["MenuDisplay"];
				return (s == null) ? MenuDisplay.TriLevel : (MenuDisplay)Enum.Parse(typeof(MenuDisplay), s);
			}

			set
			{
				ViewState["MenuDisplay"] = value.ToString();
			}
		}

		protected override void RenderContents(HtmlTextWriter output)
		{
			Webpage currentWebpage = (this.Page as PurplePage).Webpage;
			Webpage activeWebpage = null;

			if (MenuSource == MenuSource.CurrentPage)
				activeWebpage = currentWebpage;
			else
				activeWebpage = Webpages.GetPublishedWebpage(Guid.Empty);


            if (currentWebpage == null)
            {
                output.Write("No webpage");
                return;
            }

			if (MenuDisplay == MenuDisplay.Flat || MenuDisplay == MenuDisplay.FlatWithParent)
			{

				output.Write("<ul>");

				if (MenuDisplay == Purple.Controls.MenuDisplay.FlatWithParent)
				{
					output.Write("<li" + ((currentWebpage.WebpageID == activeWebpage.WebpageID) ? " class=\"" + this.SelectedCssClass  + "\"" : "") + "><a href=\"/" + activeWebpage.Url + "\">" + activeWebpage.MenuTitle + "</a></li>\n");
				}

				foreach (Webpage menuItem in activeWebpage.ChildrenInMenu)
				{
					output.Write("<li" + ((currentWebpage.WebpageID == menuItem.WebpageID) ? " class=\"" + this.SelectedCssClass + "\"" : "") + "><a href=\"/" + menuItem.Url + "\">" + menuItem.MenuTitle + "</a></li>\n");
				}

				output.Write("</ul>"); // main list
			} 
			else if (MenuDisplay == MenuDisplay.TriLevel)
			{
				Webpage parent = null;
				switch (activeWebpage.MenuType)
				{
					case MenuType.Parent:
						parent = activeWebpage;
						break;
					case MenuType.MenuItem:
						parent = activeWebpage.Parent;
						break;
					case MenuType.SubMenuItem:
						parent = activeWebpage.Parent.Parent;
						break;
				}

                if (parent  == null)
                {
                    output.Write("No parent");
                    return;
                }

				output.Write("<ul>");

                // write the top level "parent"
                output.Write("<li" + ((currentWebpage.WebpageID == parent.WebpageID) ? " class=\"" + this.SelectedCssClass + "\"" : "") + "><a href=\"/" + parent.Url + "\">" + parent.MenuTitle + "</a>");

				output.Write("<ul>"); // list for menu items

				foreach (Webpage menuItem in parent.ChildrenInMenu)
				{
                    // write normal menu item
					output.Write("<li" + ((currentWebpage.WebpageID == menuItem.WebpageID) ? " class=\"" + this.SelectedCssClass + "\"" : "") + "><a href=\"/" + menuItem.Url + "\">" + menuItem.MenuTitle + "</a>\n");

					// if the current page is the menuItem (show it's children)
					// or if the current page is a subMenuItem and it's parent is the current menuItem
                    if (currentWebpage.WebpageID == menuItem.WebpageID || currentWebpage.ParentID == menuItem.WebpageID)
					{
                        if (menuItem.ChildrenInMenu.Count > 0)
                        {
                            output.Write("<ul>\n");

                            foreach (Webpage subMenuItem in menuItem.ChildrenInMenu)
                            {
                                output.Write("<li" + ((currentWebpage.WebpageID == subMenuItem.WebpageID) ? " class=\"" + this.SelectedCssClass + "\"" : "") + "><a href=\"/" + subMenuItem.Url + "\">" + subMenuItem.MenuTitle + "</a></li>\n");
                            }
                            output.Write("</ul>\n");
                        }
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
}
