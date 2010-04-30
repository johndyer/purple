using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Specialized;
using System.IO;
using System.Xml;


namespace Purple.Core
{
    public class PurpleXmlDataProvider : PurpleDataProvider
    {


		private string _basePath = string.Empty;
		private Dictionary<string, Guid> _webpagesUrlDictionary;
		private Dictionary<Guid, WebpageUrlInfo> _webpagesIDDictionary;

        public override void Initialize(string name, NameValueCollection config)
        {
			_basePath = config["basePath"];

			if (String.IsNullOrEmpty(_basePath))
			{
				_basePath = "~/cms/content/webpages/";
			}

            base.Initialize(name, config);   
        }

		#region XML Helper methods
		private XmlDocument LoadXml(string filename)
		{
			string path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath(_basePath), filename);

			XmlDocument doc = null;
			if (File.Exists(path))
			{
				doc = new XmlDocument();
				doc.Load(path);
			}

			return doc;
		}

		private void SaveXml(XmlDocument doc, string filename)
		{
			string path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath(_basePath), filename);

			doc.Save(path);			
		}

		private string GetAttributeString(XmlNode node, string attributeName, string defaultValue)
		{
			string value = defaultValue;
			if (node.Attributes[attributeName] != null)
			{
				value = node.Attributes[attributeName].Value;
			}
			return value;
		}

		private int GetAttributeInt(XmlNode node, string attributeName, int defaultValue) {
			int value = defaultValue;
			if (node.Attributes[attributeName] != null)
			{
				if (!Int32.TryParse(node.Attributes[attributeName].Value, out value))
					value = defaultValue;
			}
			return value;
		}

		private DateTime GetAttributeDateTime(XmlNode node, string attributeName, DateTime defaultValue)
		{
			DateTime value = defaultValue;
			if (node.Attributes[attributeName] != null)
			{
				if (!DateTime.TryParse(node.Attributes[attributeName].Value, out value))
					value = defaultValue;
			}
			return value;
		}

		private Guid GetAttributeGuid(XmlNode node, string attributeName, Guid defaultValue)
		{
			Guid value = defaultValue;
			if (node.Attributes[attributeName] != null)
			{
				string guidText = node.Attributes[attributeName].Value;
				if (!Guid.TryParse(guidText, out value))
					value = defaultValue;
			}
			return value;
		}

		private DateTime GetAttributeDate(XmlNode node, string attributeName, DateTime defaultValue)
		{
			DateTime value = defaultValue;
			if (node.Attributes[attributeName] != null)
			{
				if (!DateTime.TryParse(node.Attributes[attributeName].Value, out value))
					value = defaultValue;
			}
			return value;
		}

		private bool GetAttributeBool(XmlNode node, string attributeName, bool defaultValue)
		{
			bool value = defaultValue;
			if (node.Attributes[attributeName] != null)
			{
				if (!Boolean.TryParse(node.Attributes[attributeName].Value, out value))
					value = defaultValue;
			}
			return value;
		}

		private string GetSubNodeString(XmlNode node, string path, string defaultValue)
		{
			string value = defaultValue;
			XmlNode subNode = node.SelectSingleNode(path);
			if (subNode != null)
				value = subNode.InnerText;
			return value;
		}

		private int GetSubNodeInt(XmlNode node, string path, int defaultValue)
		{
			int value = defaultValue;
			XmlNode subNode = node.SelectSingleNode(path);
			if (subNode == null || !Int32.TryParse(subNode.InnerText, out value))
				value = defaultValue;
			return value;
		}

		private bool GetSubNodeBoolean(XmlNode node, string path, bool defaultValue)
		{
			bool value = defaultValue;
			XmlNode subNode = node.SelectSingleNode(path);
			if (subNode == null || !Boolean.TryParse(subNode.InnerText, out value))
				value = defaultValue ;
			return value;
		}
		#endregion

		private void ProcessRevisionXml(XmlNode node, Webpage webpage) {

			webpage.RevisionID = GetAttributeGuid(node, "revisionid", Guid.Empty);
			webpage.IsPublished = GetAttributeBool(node, "ispublished", false);
			webpage.RevisedDate = GetAttributeDateTime(node, "reviseddate", DateTime.MinValue);
			webpage.RevisedByUsername = GetAttributeString(node, "revisedbyusername", "");

			webpage.Title = GetSubNodeString(node, "meta/title", "");
			webpage.MenuTitle = GetSubNodeString(node, "meta/menutitle", "");
			webpage.MetaDescription = GetSubNodeString(node, "meta/metadescription", "");
			webpage.MetaKeywords = GetSubNodeString(node, "meta/metakeywords", "");
			webpage.Filename = GetSubNodeString(node, "meta/filename", "");
			webpage.MasterPageFilename = GetSubNodeString(node, "meta/masterpagefilename", "");
			webpage.ForceSsl = GetSubNodeBoolean(node, "meta/forcessl", false);
			webpage.ShowInMenu = GetSubNodeBoolean(node, "meta/showinmenu", true);
			webpage.SortOrder = GetSubNodeInt(node, "meta/sortorder", 0);			
			webpage.MenuType = (MenuType) GetSubNodeInt(node, "meta/menutype", 0);
			webpage.FullUrl = GetSubNodeString(node, "meta/fullurl", "");
			webpage.Editors = GetSubNodeString(node, "meta/editors", "");

			
			// process areas
			XmlNodeList areaNodes = node.SelectNodes("areas/area");
			foreach (XmlNode areaNode in areaNodes)
			{
				WebpageArea area = new WebpageArea();
				area.WebpageID = webpage.WebpageID;
				
				area.ContentPlaceHolderID = GetAttributeString(areaNode, "contentplaceholderid", "");
				area.ControlName = GetAttributeString(areaNode, "controlname", "");
				area.SortOrder = GetAttributeInt(areaNode, "sortoder", 9999);		
				area.ContentHtml = areaNode.InnerText;

				webpage.Areas.Add(area);
			}
		}


        public override List<WebpageUrlInfo> GetWebpageUrls()
        {
			List<WebpageUrlInfo> urls = new List<WebpageUrlInfo>();
			_webpagesUrlDictionary = new Dictionary<string, Guid>();
			_webpagesIDDictionary = new Dictionary<Guid, WebpageUrlInfo>();

            // load in pages.xml
			XmlDocument doc = LoadXml("pages.xml");
			XmlNodeList pages = doc.SelectNodes("//page");

			foreach (XmlNode node in pages)
			{
				WebpageUrlInfo url = new WebpageUrlInfo()
				{
					WebpageID = GetAttributeGuid(node, "webpageid", Guid.Empty),
					ParentID = GetAttributeGuid(node, "parentid", Guid.Empty),
					Url = GetAttributeString(node, "url", ""),
					Title = GetAttributeString(node, "title", ""),
					MenuTitle = GetAttributeString(node, "menutitle", "")
				};

				urls.Add(url);
				_webpagesUrlDictionary.Add(url.Url, url.WebpageID);
				_webpagesIDDictionary.Add(url.WebpageID, url);
			}

			return urls;
        }

        public override Guid AddWebpage(Webpage webpage)
        {
			// check for existing URL
			GetWebpageUrls();
			if (_webpagesUrlDictionary.ContainsKey(webpage.Url))
				throw new Exception(string.Format("Page with URL '{0}' already exists", webpage.Url));


			webpage.WebpageID = Guid.NewGuid();


			// add to main listing
			XmlDocument doc = LoadXml("pages.xml");
			XmlNode pageNode = doc.CreateNode(XmlNodeType.Element, "page", "");
			
			XmlAttribute webpageid = doc.CreateAttribute("webpageid");
			webpageid.Value = webpage.WebpageID.ToString();
			pageNode.Attributes.Append(webpageid);

			XmlAttribute parentwebpageid = doc.CreateAttribute("parentid");
			parentwebpageid.Value = webpage.ParentID.ToString();
			pageNode.Attributes.Append(parentwebpageid);

			XmlAttribute url = doc.CreateAttribute("url");
			url.Value = webpage.Url.ToString();
			pageNode.Attributes.Append(url);

			XmlAttribute title = doc.CreateAttribute("title");
			title.Value = webpage.Title.ToString();
			pageNode.Attributes.Append(title);

			XmlAttribute menuTitle = doc.CreateAttribute("menutitle");
			menuTitle.Value = webpage.MenuTitle.ToString();
			pageNode.Attributes.Append(menuTitle);
			
			doc.SelectSingleNode("//pages").AppendChild(pageNode);
			doc.Save(Path.Combine(System.Web.HttpContext.Current.Server.MapPath(_basePath), "pages.xml"));


			// creat the new page
			FileStream fileStream = new FileStream(
					Path.Combine(System.Web.HttpContext.Current.Server.MapPath(_basePath), webpage.WebpageID.ToString() + ".xml"), 
					FileMode.Create);

			CreateWebpageXml(webpage, fileStream);
			fileStream.Close();

			return webpage.WebpageID;
        }

		private void CreateWebpageXml(Webpage webpage, Stream stream) {
			
			// create new document
			XmlTextWriter writer = new XmlTextWriter(stream, Encoding.Unicode);
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument();
			writer.WriteStartElement("page");

			writer.WriteStartElement("revision");
			writer.WriteAttributeString("revisionid", webpage.RevisionID.ToString());
			writer.WriteAttributeString("ispublished", webpage.IsPublished.ToString().ToLower());
			writer.WriteAttributeString("createdate", webpage.RevisedDate.ToUniversalTime().ToString());
			writer.WriteAttributeString("revisedbyusername", webpage.RevisedByUsername);

			writer.WriteStartElement("meta");
			writer.WriteElementString("title", webpage.Title);
			writer.WriteElementString("menutitle", webpage.MenuTitle);
			writer.WriteElementString("metadescription", webpage.MetaDescription);
			writer.WriteElementString("metakeywords", webpage.MetaKeywords);
			writer.WriteElementString("masterpagefilename", webpage.MasterPageFilename);
			writer.WriteElementString("filename", webpage.Filename);
			writer.WriteElementString("forcessl", webpage.ForceSsl.ToString().ToLower());
			writer.WriteElementString("showinmenu", webpage.ShowInMenu.ToString().ToLower());
			writer.WriteElementString("menutype", ((int)webpage.MenuType).ToString().ToLower());
			writer.WriteElementString("sortorder", webpage.SortOrder.ToString().ToLower());
			writer.WriteElementString("fullurl", webpage.FullUrl);
			writer.WriteElementString("editors", webpage.Editors);
			writer.WriteEndElement(); // meta


			writer.WriteStartElement("areas");
			foreach (WebpageArea area in webpage.Areas)
			{
				writer.WriteStartElement("area");
				writer.WriteAttributeString("contentplaceholderid", area.ContentPlaceHolderID);
				writer.WriteAttributeString("controlname", area.ControlName);

				string html = area.ContentHtml;
				// TEMP: TinyMCE adds CDATA tags which break XML
				html.Replace("<![CDATA[", "");
				html.Replace("]]>", "");

				if (!String.IsNullOrEmpty(html) )
					writer.WriteCData(html);
				
				writer.WriteEndElement(); // areas
			}
			writer.WriteEndElement(); // areas

			writer.WriteEndElement(); // revision

			writer.WriteEndElement(); // page
			writer.WriteEndDocument();
			writer.Flush();
		}


        public override void DeleteWebpage(Guid webpageID)
        {
            throw new NotImplementedException();
        }

        public override void UpdateWebpage(Webpage webpage)
        {
			webpage.IsPublished = true;			

			// update the URL in the main index
			XmlDocument pagesDoc = LoadXml("pages.xml");
			XmlNode pageNode = pagesDoc.SelectSingleNode("//page[@webpageid='" + webpage.WebpageID.ToString() + "']");
			if (pageNode.Attributes["url"] != null) {
				pageNode.Attributes["url"].Value = webpage.Url;
			} else {
				XmlAttribute attr = pagesDoc.CreateAttribute("url");
				attr.Value = webpage.Url.ToString();
				pageNode.Attributes.Append(attr);
			}		

			
			if (pageNode.Attributes["title"] != null)
			{
				pageNode.Attributes["title"].Value = webpage.Url;
			}
			else
			{
				XmlAttribute attr = pagesDoc.CreateAttribute("url");
				attr.Value = webpage.Url.ToString();
				pageNode.Attributes.Append(attr);
			}
			if (pageNode.Attributes["menutitle"] != null)
			{
				pageNode.Attributes["menutitle"].Value = webpage.Url;
			}
			else
			{
				XmlAttribute attr = pagesDoc.CreateAttribute("menutitle");
				attr.Value = webpage.MenuType.ToString();
				pageNode.Attributes.Append(attr);
			}
			SaveXml(pagesDoc, "pages.xml");

            // open existing document
			XmlDocument doc = LoadXml(webpage.WebpageID.ToString() + ".xml");

			// set all revisions to older
			if (webpage.IsPublished)
			{
				XmlNodeList revisions = doc.SelectNodes("//revision");
				foreach (XmlNode revision in revisions)
				{
					revision.Attributes["ispublished"].Value = "false";
				}
			}

			// create new page
			webpage.RevisionID = Guid.NewGuid();
			MemoryStream stream = new MemoryStream();
			CreateWebpageXml(webpage, stream);

			// load as XML
			XmlDocument newpageDoc = new XmlDocument();
			stream.Seek(0, SeekOrigin.Begin);
			newpageDoc.Load(stream);
			stream.Close();

			// find the new revision
			XmlNode newRevision = newpageDoc.SelectSingleNode("//revision");
			newRevision = doc.ImportNode(newRevision, true);

			// add the new revision to the existing doc (at the top)
			doc.SelectSingleNode("//page").PrependChild(newRevision);

			// save to disk
			doc.Save(Path.Combine(System.Web.HttpContext.Current.Server.MapPath(_basePath), webpage.WebpageID.ToString() + ".xml"));
        }

        public override Webpage GetWebpage(Guid webpageID, Guid revisionID)
        {
			Webpage webpage = null;


			XmlDocument doc = LoadXml("" + webpageID.ToString() + ".xml");
			if (doc != null)
			{
				webpage = new Webpage();
				webpage.WebpageID = webpageID;
				webpage.IsSiteRoot = (webpageID == Guid.Empty);
				XmlNode revisionNode = doc.SelectSingleNode("//revision[@revisionid='" + revisionID.ToString() + "']");

				if (revisionNode != null)
					ProcessRevisionXml(revisionNode, webpage);
				else
					webpage = null;
			}

			return webpage;
        }

        public override Webpage GetPublishedWebpage(Guid webpageID)
        {
			Webpage webpage = null;

            // refresh index
            if (_webpagesUrlDictionary == null)
                GetWebpageUrls();

			XmlDocument doc = LoadXml("" + webpageID.ToString() + ".xml");
            if (doc != null && _webpagesIDDictionary != null)
			{
				WebpageUrlInfo urlInfo = _webpagesIDDictionary[webpageID];

				webpage = new Webpage();
				webpage.WebpageID = webpageID;
				webpage.IsSiteRoot = (webpageID == Guid.Empty);
				webpage.ParentID = urlInfo.ParentID;
				webpage.Url = urlInfo.Url;

				XmlNode revisionNode = doc.SelectSingleNode("//revision[@ispublished='true']");

				if (revisionNode == null)
					revisionNode = doc.SelectNodes("//revision")[0];

				ProcessRevisionXml(revisionNode, webpage);
			}

			return webpage;
        }

        public override Webpage GetPublishedWebpage(string url)
        {
			Guid webpageID = Guid.Empty;

			// refresh index
			if (_webpagesUrlDictionary == null)
				GetWebpageUrls();
			
			if (!String.IsNullOrWhiteSpace(url) && _webpagesUrlDictionary.ContainsKey(url))
				webpageID = _webpagesUrlDictionary[url];				


			return GetPublishedWebpage(webpageID);
        }

		public override List<Webpage> GetWebpageRevisions(Guid webpageID)
		{
            // refresh index
            if (_webpagesUrlDictionary == null)
                GetWebpageUrls();

			List<Webpage> revisions = new List<Webpage>();

			XmlDocument doc = LoadXml("" + webpageID.ToString() + ".xml");
            if (doc != null && _webpagesIDDictionary != null)
			{
				WebpageUrlInfo urlInfo = _webpagesIDDictionary[webpageID];
				
				XmlNodeList revisionNodes = doc.SelectNodes("//revision");

				foreach (XmlNode revisionNode in revisionNodes)
				{
					Webpage webpage = new Webpage();
					webpage.WebpageID = webpageID;
					webpage.IsSiteRoot = (webpageID == Guid.Empty);
					webpage.ParentID = urlInfo.ParentID;
					webpage.Url = urlInfo.Url;

					ProcessRevisionXml(revisionNode, webpage);

					revisions.Add(webpage);
				}				
			}

			return revisions;
		}

		public override Webpage GetParentWebpage(Guid webpageID)
		{
			Webpage webpage = null;

			WebpageUrlInfo urlInfo = _webpagesIDDictionary[webpageID];
			if (urlInfo != null)
				webpage = GetPublishedWebpage(urlInfo.ParentID);

			return webpage;
		}

		public override List<Webpage> GetChildWebpages(Guid webpageID)
		{
			List<Webpage> children = new List<Webpage>();
			
			foreach (WebpageUrlInfo url in _webpagesIDDictionary.Values)
			{
				if (url.ParentID == webpageID && url.WebpageID != webpageID)
				{
					children.Add(GetPublishedWebpage(url.WebpageID));
				}
			}

			children.Sort(delegate(Webpage a, Webpage b) { 
                int sortValue = a.SortOrder.CompareTo(b.SortOrder);
                if (sortValue == 0)
                    sortValue = a.MenuTitle.CompareTo(b.MenuTitle);

                return sortValue;
            });

			return children;
		}

		#region Url Redirects
		private Dictionary<string, UrlRedirect> _redirectUrlDictionary;
		private Dictionary<Guid, UrlRedirect> _redirectIDDictionary;

		private void ReloadRedirectDictionaries()
		{
			_redirectUrlDictionary = new Dictionary<string, UrlRedirect>();
			_redirectIDDictionary = new Dictionary<Guid, UrlRedirect>();

			List<UrlRedirect> redirects = GetUrlRedirects();

			foreach (UrlRedirect redirect in redirects)
			{
				_redirectUrlDictionary.Add(redirect.FromUrl, redirect);
				_redirectIDDictionary.Add(redirect.RedirectID, redirect);
			}
		}

		private UrlRedirect UrlRedirectFromNode(XmlNode redirectNode) {
			return new UrlRedirect()
					{
						RedirectID = GetAttributeGuid(redirectNode, "type", Guid.Empty),
						Title = GetAttributeString(redirectNode, "title", string.Empty),
						IsActive = GetAttributeBool(redirectNode, "isactive", false),
						IsPermanent = GetAttributeBool(redirectNode, "ispermanent", false),
						FromUrl = GetAttributeString(redirectNode, "fromurl", string.Empty),
						ToUrl = GetAttributeString(redirectNode, "tourl", string.Empty),
						Description = redirectNode.InnerText						
					};
		}

		public override List<UrlRedirect> GetUrlRedirects()
		{
			List<UrlRedirect> redirects = new List<UrlRedirect>();

			XmlDocument doc = LoadXml("redirects.xml");

			if (doc != null)
			{
				XmlNodeList redirectNodes = doc.SelectNodes("//redirect");
				foreach (XmlNode redirectNode in redirectNodes)
				{
					redirects.Add(UrlRedirectFromNode(redirectNode));
				}
			}

			return redirects;
		}


		public override UrlRedirect GetUrlRedirect(string fromUrl)
		{
			UrlRedirect redirect = null;

			if (_redirectUrlDictionary == null)
				ReloadRedirectDictionaries();

			if (_redirectUrlDictionary.ContainsKey(fromUrl))
				redirect = _redirectUrlDictionary[fromUrl];

			return redirect;
		}

		public override UrlRedirect GetUrlRedirect(Guid redirectID)
		{
			UrlRedirect redirect = null;

			if (_redirectIDDictionary == null)
				ReloadRedirectDictionaries();

			if (_redirectIDDictionary.ContainsKey(redirectID))
				redirect = _redirectIDDictionary[redirectID];

			return redirect;
		}

		public override void AddUrlRedirect(UrlRedirect urlRedirect)
		{
			throw new NotImplementedException();
		}

		public override void UpdateUrlRedirect(UrlRedirect urlRedirect)
		{
			throw new NotImplementedException();
		}

		public override void DeleteUrlRedirect(UrlRedirect urlRedirect)
		{
			throw new NotImplementedException();
		}
		#endregion


		#region Custom Routes
		public override List<CustomRouteEntry> GetCustomRouteEntries()
		{
			List<CustomRouteEntry> routes = new List<CustomRouteEntry>();

			XmlDocument doc = LoadXml("routehandlers.xml");

			if (doc != null)
			{
				XmlNodeList routeNodes = doc.SelectNodes("//route");
				foreach (XmlNode routeNode in routeNodes)
				{
					routes.Add(new CustomRouteEntry()
					{
						Name = GetAttributeString(routeNode, "name", string.Empty),
						Url = GetAttributeString(routeNode, "url", string.Empty),
						Type = GetAttributeString(routeNode, "type", string.Empty)
					});
				}
			}

			return routes;
		}

		public override void AddCustomRouteEntry(CustomRouteEntry customRouteEntry)
		{
			throw new NotImplementedException();
		}

		public override void UpdateCustomRouteEntry(CustomRouteEntry customRouteEntry)
		{
			throw new NotImplementedException();
		}

		public override void DeleteCustomRouteEntry(string routeName)
		{
			throw new NotImplementedException();
		}


		public override CustomRouteEntry GetCustomRouteEntry(string routeName)
		{
			throw new NotImplementedException();
		}
		#endregion

	}
}