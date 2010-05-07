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


    [ToolboxData("<{0}:BreadCrumbs runat=server></{0}:BreadCrumbs>")]
    public class BreadCrumbs : Control
	{
		

		protected override void Render(HtmlTextWriter output)
		{
			Webpage currentWebpage = (this.Page as PurplePage).Webpage;

            output.Write("<ul>");

            string items = "<li><span class=\"current\">" + currentWebpage.MenuTitle + "</span></li>\n";

            Webpage parent = currentWebpage.Parent;
            while (parent != null)
            {
                items = "<li" + ((parent.IsSiteRoot) ? " class=\"first\"" : "") + "><a href=\"" + parent.Url + "\">" + parent.MenuTitle + "</a></li>\n" + items;

                if (parent.IsSiteRoot)
                    break;

                parent = parent.Parent;
            }

            output.Write(items);
            output.Write("</ul>");
		}
	}
}
