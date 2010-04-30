using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;

namespace Purple.Core
{
	public enum FormMode
	{
		/// <summary>
		/// Does not check or attempt to add <form runat="server" />
		/// </summary>
		None,
		/// <summary>
		/// Requires <body runat="server"> to add form around all controls
		/// </summary>
		WholePage,
		/// <summary>
		/// Adds a <form runat="Server"></form> inside the current <see cref="System.Web.UI.ContentPlaceHolder"/>
		/// </summary>
		Container
	}

	public enum PageMode
	{
		/// <summary>
		/// Builds teh page
		/// </summary>
		Render,
		/// <summary>
		/// Allows the page to be edited
		/// </summary>
		Edit,
		/// <summary>
		/// creating a new page 
		/// </summary>
		NewPage
	}

	public class PurplePage : Page
	{
		//public string Url { get; set; }
		public Webpage Webpage { get; set; }

#region Admin controls
		private Control AdminControl { get; set; }

		private TextBox webpageFilename;
		private TextBox webpagePageTitle;
		private TextBox webpageMenuTitle;
		private TextBox webpageMetaDescription;
		private TextBox webpageMetaKeywords;
		private Label webpageFilenamePrefix;
		private TextBox webpageUrl;
		private CheckBox webpageForceSsl;
		private CheckBox webpageShowInMenu;
		private TextBox webpageSortOrder;
		private ListControl webpageMenuType;
		private CheckBoxList webpageEditors;
		private LinkButton webpageSaveButton;
		private ListControl webpageMasterPageFilename;

		private Button webpageAddButton;
		private Repeater webpageVersionsRepeater;
#endregion


		#region Page <html> elements
		private HtmlForm _form = null;
		
		private Control _body = null;
		private Control Body
		{
			get
			{
				if (_body == null)
				{
					foreach (Control c in Master.Controls)
					{
						if (c is HtmlGenericControl && ((HtmlGenericControl)c).TagName.ToLower() == "body")
						{
							_body = c;
							break;
						}
					}
				}

				if (_body == null)
					throw new Exception("The masterpage must have <body runat=\"server\"> for the Admin controls to work correctly.");

				return _body;
			}
		}
		#endregion

		private List<ContentPlaceHolder> ContentPlaceHolders { get; set; }


		protected override void OnPreInit(EventArgs e)
		{	
			// set the master page
			this.MasterPageFile = PurpleSettings.CmsPath + "content/masterpages/" + ((this.Webpage != null && !String.IsNullOrEmpty(this.Webpage.MasterPageFilename)) ? this.Webpage.MasterPageFilename : PurpleSettings.DefaultMasterPageFilename);
		}



		protected override void OnInit(EventArgs e)
		{
			FormMode formMode = FormMode.Container;
			PageMode pageMode = PageMode.Render;

			// local variable
			Webpage webpage = this.Webpage;

			if ((Request.QueryString["pagemode"] + "").ToLower() == "edit")
				pageMode = PageMode.Edit;
			//else if (Request.QueryString["pagemode"] + "" == "newpage")
			//	pageMode = PageMode.NewPage;


			bool userHasPermissions = GetUserPermissions();

			if (!userHasPermissions)
				pageMode = PageMode.Render;


			// get and clear place holders
			ContentPlaceHolders = FindControls<ContentPlaceHolder>(this.Body);

			if (ContentPlaceHolders.Count == 0)
				throw new Exception("The masterpage '{0}' has no <asp:Content runat=\"Server\" /> controls");

			ContentPlaceHolders.ForEach(p => p.Controls.Clear());

			// update meta data
			this.Title = webpage.Title + ((!webpage.IsSiteRoot) ? PurpleSettings.PageSuffix : string.Empty);
			this.MetaDescription = webpage.MetaDescription;
			this.MetaKeywords = webpage.MetaKeywords;

			if (userHasPermissions)
			{
				HtmlLink adminStylesLink = new HtmlLink() { Href = PurpleSettings.CmsPath + "assets/admincontrols/admin.css" };
				adminStylesLink.Attributes.Add("rel", "stylesheet");
				this.Header.Controls.Add(adminStylesLink);
			}


			if (pageMode == PageMode.Render)
			{
				// add the admin controls first so the form is already there
				if (userHasPermissions)
				{

					AddFormToBody();

					// add admin controls				
					AdminControl = LoadControl(PurpleSettings.CmsPath + "assets/admincontrols/adminpage.ascx");
					FindAdminControls();

					webpageAddButton.Click += new EventHandler(PurplePageAddButton_Click);

					// fill in the details
					webpageFilenamePrefix.Text = (!webpage.IsSiteRoot) ? webpage.Url + "/" : "";

					// versions					
					webpageVersionsRepeater.DataSource = Webpages.GetWebpageRevision(webpage.WebpageID);
					webpageVersionsRepeater.DataBind();
					
					_form.Controls.Add(AdminControl);
				}


				// go through the webpage areas
				foreach (WebpageArea area in webpage.Areas)
				{
					// get the right placeholder
					ContentPlaceHolder placeholder = ContentPlaceHolders.Find(c => c.ID == area.ContentPlaceHolderID);

					// use default
					if (placeholder == null)
						placeholder = ContentPlaceHolders[0];


					if (!String.IsNullOrWhiteSpace(area.ControlName))
					{

						// find the ASCX file
						string controlVPath = PurpleSettings.CmsPath + "/content/controls/" + area.ControlName;
						string controlPath = Server.MapPath(controlVPath);

						if (File.Exists(controlPath))
						{
							Control control = LoadControl(controlVPath);
							bool requiresForm = CheckRequiresForm(placeholder, control); ;
							if (requiresForm)
							{
								if (formMode == FormMode.Container)
								{
									System.Web.UI.HtmlControls.HtmlForm form = new System.Web.UI.HtmlControls.HtmlForm();
									placeholder.Controls.Add(form);
									form.Controls.Add(control);
								}
								else if (formMode == FormMode.WholePage)
								{
									AddFormToBody();
								}
							}
							else
							{
								placeholder.Controls.Add(control);
							}
						}
						else
						{
							placeholder.Controls.Add(new LiteralControl("<p>No control at :" + controlVPath + "</p>"));
						}

					}
					else
					{
						// just fill with HTML
						placeholder.Controls.Add(new LiteralControl(area.ContentHtml));
					}


				}


			}
			else if (pageMode == PageMode.Edit)
			{


				// just in case
				AddFormToBody();

				// main edit control
				AdminControl = LoadControl(PurpleSettings.CmsPath + "assets/admincontrols/editpage.ascx");
				_form.Controls.Add(AdminControl);

				// attach all the controls
				FindAdminControls();

				// special cases
				if (webpage.IsSiteRoot)
				{
					webpageFilename.Enabled = false;
					webpageUrl.Enabled = false;
				}


				// main attributes
				webpageFilename.Text = webpage.Filename;
				webpagePageTitle.Text = webpage.Title;
				webpageMenuTitle.Text = webpage.MenuTitle;
				webpageMetaDescription.Text = webpage.MetaDescription;
				webpageMetaKeywords.Text = webpage.MetaKeywords;
				webpageFilenamePrefix.Text = (!webpage.IsSiteRoot && !webpage.Parent.IsSiteRoot) ? webpage.Parent.Url + "/" : "";
				webpageUrl.Text = webpage.Url;
				webpageForceSsl.Checked = webpage.ForceSsl;
				webpageShowInMenu.Checked = webpage.ShowInMenu;
				webpageSortOrder.Text = webpage.SortOrder.ToString() ;				
				
				// menu type enum
				
				string[] menuTypeNames = Enum.GetNames(typeof(MenuType));
				Array menuTypeValues = Enum.GetValues(typeof(MenuType));
				for (int i = 0; i < menuTypeNames.Length; i++ )
				{
					webpageMenuType.Items.Add(new ListItem(menuTypeNames[i])); //, menuTypeValues.GetValue(i).ToString()));
				}
				if (webpageMenuType.Items.FindByValue(webpage.MenuType.ToString()) != null)
					webpageMenuType.Items.FindByValue(webpage.MenuType.ToString()).Selected = true;

				// master pages
				List<string> masterPages = new List<string>();
				UtilityMethods.FindFilesRecursive(masterPages, "*.master", Server.MapPath(PurpleSettings.CmsPath + "content/masterpages/"), true);
				masterPages.Sort();
				
				//webpageMasterPageFilename.DataSource = masterPages;
			//	webpageMasterPageFilename.DataBind();
				foreach (string masterPage in masterPages)
				{
					webpageMasterPageFilename.Items.Add(new ListItem("<img src=\"" + ResolveClientUrl(PurpleSettings.CmsPath + "content/masterpages/") + masterPage + ".png\" /><span>" + masterPage + "</span>", masterPage));
				}
				if (webpageMasterPageFilename.Items.FindByValue(webpage.MasterPageFilename.Replace(".master","")) != null)
					webpageMasterPageFilename.Items.FindByValue(webpage.MasterPageFilename.Replace(".master", "")).Selected = true;

				// permissions
				string[] allEditors = System.Web.Security.Roles.GetUsersInRole(PurpleSettings.RoleEditor);				
				webpageEditors.DataSource = allEditors;
				webpageEditors.DataBind();

				List<string> pageEditors = webpage.Editors.Split(new char[] { ',' }).ToList<String>();
				foreach (ListItem li in webpageEditors.Items) {
					if (pageEditors.Contains(li.Value))
						li.Selected = true;
				}


				// attach save event
				webpageSaveButton.Click += new EventHandler(PurplePageSaveButton_Click);


				// WEBPAGE AREAS

				
				// list of control folders
				string controlBase = Server.MapPath(PurpleSettings.CmsPath + "content/controls/");
				List<string> controlFolders = new List<string>();
				UtilityMethods.FindFoldersRecursive(controlFolders, controlBase);

				// list of controls
				List<string> availableControls = new List<string>();
				UtilityMethods.FindFilesRecursive(availableControls, "*.ascx", controlBase, true);


				// go through the webpage areas
				foreach (WebpageArea area in webpage.Areas)
				{
					// get the right placeholder
					ContentPlaceHolder placeholder = ContentPlaceHolders.Find(c => c.ID == area.ContentPlaceHolderID);

					// use default
					if (placeholder == null)
						placeholder = ContentPlaceHolders[0];


					Control areaControl = LoadControl(PurpleSettings.CmsPath + "assets/admincontrols/editarea.ascx");

					// grab the html area
					TextBox htmlContent = areaControl.FindControl("HtmlContent") as TextBox;
					htmlContent.Text = area.ContentHtml;


					// fill in the *.ascx controls
					DropDownList controlName = areaControl.FindControl("ControlName") as DropDownList;
					controlName.DataSource = availableControls;
					controlName.DataBind();
					controlName.Items.Insert(0, new ListItem("-- No Control (use HTML) --", ""));

					if (!String.IsNullOrWhiteSpace(area.ControlName) && controlName.Items.FindByValue(area.ControlName.Replace(".ascx", "")) != null)
						controlName.Items.FindByValue(area.ControlName.Replace(".ascx", "")).Selected = true;

					// fill in the upload folders
					DropDownList controlUploadFolder = areaControl.FindControl("ControlUploadFolder") as DropDownList;
					controlUploadFolder.DataSource = controlFolders;
					controlUploadFolder.DataBind();
					controlUploadFolder.Items.Insert(0, new ListItem("-- Root Folder --", ""));

					// attach event to the upload button
					(areaControl.FindControl("ControlUploadButton") as Button).Click += new EventHandler(ControlUploadButton_Click);


					// add this control
					placeholder.Controls.Add(areaControl);


				}

			}

			base.OnInit(e);
		}

		private void FindAdminControls()
		{
			webpageFilename = AdminControl.FindControl("WebpageFilename") as TextBox;
			webpagePageTitle = AdminControl.FindControl("WebpagePageTitle") as TextBox;
			webpageMenuTitle = AdminControl.FindControl("WebpageMenuTitle") as TextBox;
			webpageMetaDescription = AdminControl.FindControl("WebpageMetaDescription") as TextBox;
			webpageMetaKeywords = AdminControl.FindControl("WebpageMetaKeywords") as TextBox;
			webpageFilenamePrefix = AdminControl.FindControl("WebpageFilenamePrefix") as Label;
			webpageUrl = AdminControl.FindControl("WebpageUrl") as TextBox;
			webpageForceSsl = AdminControl.FindControl("WebpageForceSsl") as CheckBox;
			webpageShowInMenu = AdminControl.FindControl("WebpageShowInMenu") as CheckBox;
			webpageSortOrder = AdminControl.FindControl("WebpageSortOrder") as TextBox;
			webpageMenuType = AdminControl.FindControl("WebpageMenuType") as ListControl;
			webpageEditors = AdminControl.FindControl("WebpageEditors") as CheckBoxList;
			webpageSaveButton = AdminControl.FindControl("WebpageSaveButton") as LinkButton;
			webpageMasterPageFilename = AdminControl.FindControl("WebpageMasterPageFilename") as ListControl;
			webpageAddButton = AdminControl.FindControl("WebpageAddButton") as Button;
			webpageVersionsRepeater = AdminControl.FindControl("WebpageRevisionsRepeater") as Repeater;
		}

		private bool GetUserPermissions()
		{
			bool userHasPermissions = false;

			// check for super admin 
			if (User.IsInRole(PurpleSettings.RoleSuperAdmin))
				userHasPermissions = true;

			// check for page editors
			if (User.IsInRole(PurpleSettings.RoleEditor) && Webpage.Editors.Split(new char[] { ',' }).Contains<string>(User.Identity.Name))
				userHasPermissions = true;

			return userHasPermissions;
		}

		void ControlUploadButton_Click(object sender, EventArgs e)
		{
			Button uploadButton = (Button)sender;

			// find the container control
			Control adminEditArea = uploadButton.Parent;

			// find the sub controls
			FileUpload controlUploadFile = adminEditArea.FindControl("ControlUploadFile") as FileUpload;
			DropDownList controlUploadFolder = adminEditArea.FindControl("ControlUploadFolder") as DropDownList;
			DropDownList controlName = adminEditArea.FindControl("ControlName") as DropDownList;

			if (controlUploadFile.HasFile && controlUploadFile.FileName.EndsWith(".ascx"))
			{				
				string baseControlsPath = HttpContext.Current.Server.MapPath(PurpleSettings.CmsPath + "controls");

				// create path= [base]/[folder]/[file]
				string savedFilePath = Path.Combine(baseControlsPath, controlUploadFolder.SelectedValue);
				savedFilePath = Path.Combine(savedFilePath, controlUploadFile.FileName);

				// check if file exists, then save
				if (!File.Exists(savedFilePath))
				{
					controlUploadFile.SaveAs(savedFilePath);

					// remove the extension
					string listPath = savedFilePath.Substring(0, savedFilePath.LastIndexOf("."));
					// remove the base path
					listPath = listPath.Replace(baseControlsPath, "").Trim(new char[] {'\\'});

					// add it to the list, and select it
					controlName.Items.Add(new ListItem(listPath));
					controlName.SelectedIndex = controlName.Items.Count - 1;
				}

			}

		}

		void PurplePageSaveButton_Click(object sender, EventArgs e)
		{
			// What to do now?	
			Webpage webpage = this.Webpage;

			FindAdminControls();

			// update main values
			webpage.Filename = webpageFilename.Text;
			webpage.FullUrl = ((!webpage.Parent.IsSiteRoot) ? webpage.Parent.Url + "/" : "") + webpage.Filename;

			string url = webpageUrl.Text;
			// TODO: seriously need some better checking here
			webpage.Url = (!String.IsNullOrWhiteSpace(url)) ? url : webpage.FullUrl;

			webpage.Title = webpagePageTitle.Text;
			webpage.MenuTitle = webpageMenuTitle.Text;
			webpage.MetaDescription = webpageMetaDescription.Text;
			webpage.MetaKeywords = webpageMetaKeywords.Text;
			webpage.MasterPageFilename = webpageMasterPageFilename.SelectedValue + ".master";

			webpage.ForceSsl = webpageForceSsl.Checked;
			webpage.ShowInMenu = webpageShowInMenu.Checked;

			webpage.MenuType = (MenuType) Enum.Parse( typeof(MenuType), webpageMenuType.SelectedValue);

			int menuSortOrder = 0;
			if (Int32.TryParse(webpageSortOrder.Text, out menuSortOrder))
				webpage.SortOrder = menuSortOrder;

			// permissions
			List<string> pageEditors = new List<string>();
			foreach (ListItem li in webpageEditors.Items)
			{
				if (li.Selected)
					pageEditors.Add(li.Value);
			}
			webpage.Editors = String.Join(",",pageEditors.ToArray());


			// update modules
			webpage.Areas.Clear();			
			foreach (ContentPlaceHolder placeholder in ContentPlaceHolders)
			{
				int sortOrder = 1;
				// each sub control should be a editarea.ascx control
				foreach (Control control in placeholder.Controls)
				{
					WebpageArea area = new WebpageArea();

					string html = (control.FindControl("HtmlContent") as TextBox).Text;

					area.ContentPlaceHolderID = placeholder.ID;
					area.ContentHtml = html;
					DropDownList controlList = control.FindControl("ControlName") as DropDownList;
					area.ControlName = (!String.IsNullOrWhiteSpace(controlList.SelectedValue)) ? controlList.SelectedValue + ".ascx" : "";
					area.SortOrder = sortOrder;

					webpage.Areas.Add(area);
					sortOrder++;
				}
			}
			
			// save
			webpage.IsPublished = true;
			Webpages.UpdateWebpage(webpage);

			// see the finished page
			Response.Redirect("/" + webpage.Url);
		}

		void PurplePageAddButton_Click(object sender, EventArgs e)
		{
			// What to do now?	
			Webpage newpage = new Webpage();

			FindAdminControls();

			newpage.ParentID = this.Webpage.WebpageID;
			newpage.Filename = webpageFilename.Text;
			newpage.Title = webpagePageTitle.Text;
			newpage.MenuTitle = webpageMenuTitle.Text;
			newpage.Url = ((this.Webpage.IsSiteRoot) ? "" : this.Webpage.Url + "/") + newpage.Filename;
			newpage.FullUrl = newpage.Url;
			newpage.ForceSsl = false;
			newpage.ShowInMenu = true;
			newpage.MasterPageFilename = this.Webpage.MasterPageFilename; //  PurpleSettings.DefaultMasterPageFilename;

			WebpageArea area = new WebpageArea();
			area.ContentPlaceHolderID = ContentPlaceHolders[0].ID;
			area.ContentHtml = "New Page";
			newpage.Areas.Add(area);

			// save
			Webpages.AddWebpage(newpage);

			// see the finished page
			Response.Redirect("/" + newpage.Url + "?pagemode=edit");
		}

		private void AddFormToBody() {
			List<HtmlForm> forms = FindControls<HtmlForm>(this);

			if (forms.Count > 0)
			{
				_form = forms[0];
			}
			else
			{

				_form = new HtmlForm();

				if (Body != null)
				{
					// move all controls to the form
					while (Body.HasControls())
					{
						_form.Controls.Add(Body.Controls[0]);
					}
					// add form to body
					Body.Controls.Add(_form);
				}
			}
		}


		public static List<T> FindControls<T>(Control control) {
			List<T> matchingControls = new List<T>();
			Type t = typeof(T);

			FindControls<T>(control, ref matchingControls);

			return matchingControls;
		}

		


		public static void FindControls<T>(Control control, ref List<T> matchingControls)
		{
			foreach (dynamic subControl in control.Controls)
			{
				if (subControl is T)				
					matchingControls.Add( (T)subControl );
				

				// find the next level of controls
				if (subControl.HasControls())
					FindControls<T>(subControl, ref matchingControls);
			}
		}


		public static List<ContentPlaceHolder> FindContentPlaceHolders(Control control)
		{
			if (control == null)
				return new List<ContentPlaceHolder>();

			List<ContentPlaceHolder> templateRegions = new List<ContentPlaceHolder>();

			if (control.HasControls())
				FindContentPlaceHolders(control, ref templateRegions);


			

			return templateRegions;
		}

		private static void FindContentPlaceHolders(Control control, ref List<ContentPlaceHolder> templateRegions)
		{

			foreach (Control subControl in control.Controls)
			{
				if (subControl is ContentPlaceHolder && !subControl.ID.StartsWith("_"))
				{
					templateRegions.Add((ContentPlaceHolder)subControl);
				}				

				// find the next level of controls
				if (subControl.HasControls())
					FindContentPlaceHolders(subControl, ref templateRegions);
			}


		}

		private static bool HasParentControl<T>(Control control)
		{
			Control parent = control.Parent;
			while (parent != null)
			{
				if (parent is T)
					return true;
				parent = parent.Parent;
			}
			return false;
		}

		private bool CheckRequiresForm(ContentPlaceHolder container, Control control)
		{

			bool hasForm = (FindControls<HtmlForm>(this.Master).Count > 0);
			//bool hasForm = HasParentControl<HtmlForm>(container);
			if (hasForm)
			{
				return false;
			} else {
				// if there are any web controls then we need a form
				return (FindControls<IPostBackDataHandler>(control).Count > 0);
			}			
		}
	}
}
