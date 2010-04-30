using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Specialized;

namespace Purple.Core
{
    public class PurpleSqlDataProvider : PurpleDataProvider
    {
        private string _connectionString = string.Empty;

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

			if (ConfigurationManager.ConnectionStrings[config["connectionString"]] == null)
				throw new ConfigurationErrorsException("connectionString must be in the ConnectionStrings area of the web.config");

            this._connectionString = ConfigurationManager.ConnectionStrings[config["connectionString"]].ConnectionString;

            if (string.IsNullOrEmpty(this._connectionString))
                throw new ConfigurationErrorsException("connectionString must be set to the appropriate value");
        }


		public override List<WebpageUrlInfo> GetWebpageUrls()
		{
			List<WebpageUrlInfo> webpageUrls = new List<WebpageUrlInfo>();

			SqlConnection connection = new SqlConnection(this._connectionString);


			string sql = @"
SELECT 
	WebpageID, ParentID, Url, Title, MenuTitle
FROM 
	purple_WebpageRevisions
WHERE
	IsPublished = 1;";
			SqlCommand command = new SqlCommand(sql, connection);

			connection.Open();
			SqlDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				webpageUrls.Add(new WebpageUrlInfo()
				{
					Url = reader["Url"].ToString(),
					ParentID = new Guid(reader["ParentID"].ToString()),
					WebpageID = new Guid(reader["WebpageID"].ToString()),
					Title = reader["Title"].ToString(),
					MenuTitle = reader["MenuTitle"].ToString()
				});
			}
			reader.Close();
			connection.Close();
			command.Dispose();


			return webpageUrls;
		}

        public override Guid AddWebpage(Webpage webpage)
        {
            // add main entry
			webpage.WebpageID = Guid.NewGuid();

			SqlConnection connection = new SqlConnection(this._connectionString);

			string sql = @"
INSERT INTO 
	purple_Webpages
		(WebpageID, ParentID, Title, MenuTitle)
VALUES
		(@WebpageID, @ParentID, Title, MenuTitle);";

			SqlCommand command = new SqlCommand(sql, connection);
			command.Parameters.Add("@WebpageID", SqlDbType.UniqueIdentifier).Value = webpage.WebpageID;
			command.Parameters.Add("@ParentID", SqlDbType.UniqueIdentifier).Value = webpage.ParentID;
			command.Parameters.Add("@Title", SqlDbType.NVarChar).Value = webpage.Title;
			command.Parameters.Add("@MenuTitle", SqlDbType.NVarChar).Value = webpage.MenuTitle;

			connection.Open();
			command.ExecuteNonQuery();
			connection.Close();

			// add to revision
			UpdateWebpage(webpage);

			return webpage.WebpageID;
        }



        public override void DeleteWebpage(Guid webpageID)
        {
            throw new NotImplementedException();
        }

        public override void UpdateWebpage(Webpage webpage)
        {
			webpage.RevisionID = Guid.NewGuid();

			SqlConnection connection = new SqlConnection(this._connectionString);

			string sql = @"
INSERT INTO 
	purple_WebpageRevisions
		(WebpageID, ParentID, RevisionID, Title, MenuTitle, MetaDescription, MetaKeywords, ForceSsl, IsPublished, Filename, Url, MasterPageFilename, FullUrl, MenuType, SortOrder, ShowInMenu, RevisedByUsername, RevisedDate, Editors)
VALUES
		(@WebpageID, @ParentID, @RevisionID, @Title, @MenuTitle, @MetaDescription, @MetaKeywords, @ForceSsl, @IsPublished, @Filename, @Url, @MasterPageFilename, @FullUrl, @MenuType, @SortOrder, @ShowInMenu, @RevisedByUsername, @RevisedDate, @Editors);

UPDATE 
	purple_Webpages
SET 
	Title = @Title, MenuTitle=@MenuTitle
WHERE
	WebpageID = @WebpageID;
";

			SqlCommand command = new SqlCommand(sql, connection);
			command.Parameters.Add("@WebpageID", SqlDbType.UniqueIdentifier).Value = webpage.WebpageID;
			command.Parameters.Add("@ParentID", SqlDbType.UniqueIdentifier).Value = webpage.ParentID;
			command.Parameters.Add("@RevisionID", SqlDbType.UniqueIdentifier).Value = webpage.RevisionID;
			command.Parameters.Add("@Title", SqlDbType.NVarChar).Value = webpage.Title;
			command.Parameters.Add("@MenuTitle", SqlDbType.NVarChar).Value = webpage.MenuTitle;
			command.Parameters.Add("@MetaDescription", SqlDbType.NVarChar).Value = webpage.MetaDescription;
			command.Parameters.Add("@MetaKeywords", SqlDbType.NVarChar).Value = webpage.MetaKeywords;
			command.Parameters.Add("@ForceSsl", SqlDbType.Bit).Value = webpage.ForceSsl;
			command.Parameters.Add("@IsPublished", SqlDbType.Bit).Value = webpage.IsPublished;
			command.Parameters.Add("@Filename", SqlDbType.NVarChar).Value = webpage.Filename;
			command.Parameters.Add("@Url", SqlDbType.NVarChar).Value = webpage.Url;
			command.Parameters.Add("@MasterPageFilename", SqlDbType.NVarChar).Value = webpage.MasterPageFilename;

			command.Parameters.Add("@FullUrl", SqlDbType.NVarChar).Value = webpage.FullUrl;
			command.Parameters.Add("@MenuType", SqlDbType.Int).Value = (int) webpage.MenuType;
			command.Parameters.Add("@SortOrder", SqlDbType.Int).Value = webpage.SortOrder;
			command.Parameters.Add("@ShowInMenu", SqlDbType.Bit).Value = webpage.ShowInMenu;

			command.Parameters.Add("@RevisedByUsername", SqlDbType.NVarChar).Value = webpage.RevisedByUsername;
			command.Parameters.Add("@RevisedDate", SqlDbType.DateTime).Value = webpage.RevisedDate;

			command.Parameters.Add("@Editors", SqlDbType.NVarChar).Value = webpage.Editors;

			connection.Open();
			command.ExecuteNonQuery();

			// add areas
			foreach (WebpageArea area in webpage.Areas)
			{
				area.AreaID = Guid.NewGuid();

				sql = @"
INSERT INTO 
	purple_WebpageAreas
		(AreaID, WebpageID, RevisionID, ContentPlaceHolderID, SortOrder, ControlName, ContentHtml)
VALUES
		(@AreaID, @WebpageID, @RevisionID, @ContentPlaceHolderID, @SortOrder, @ControlName, @ContentHtml);";

				command = new SqlCommand(sql, connection);
				command.Parameters.Add("@AreaID", SqlDbType.UniqueIdentifier).Value = area.AreaID;
				command.Parameters.Add("@WebpageID", SqlDbType.UniqueIdentifier).Value = webpage.WebpageID;
				command.Parameters.Add("@RevisionID", SqlDbType.UniqueIdentifier).Value = webpage.RevisionID;
				command.Parameters.Add("@ContentPlaceHolderID", SqlDbType.NVarChar).Value = area.ContentPlaceHolderID;
				command.Parameters.Add("@SortOrder", SqlDbType.NVarChar).Value = area.SortOrder;
				command.Parameters.Add("@ControlName", SqlDbType.NVarChar).Value = area.ControlName;
				command.Parameters.Add("@ContentHtml", SqlDbType.NText).Value = area.ContentHtml;

				command.ExecuteNonQuery();
			}

			connection.Close();
			command.Dispose();		
        }

        public override Webpage GetWebpage(Guid webpageID, Guid revisionID)
        {
			// add main entry
			Webpage webpage = null;

			SqlConnection connection = new SqlConnection(this._connectionString);

			string sql = @"
SELECT 
	* 
FROM 
	purple_WebpageRevisions
WHERE
	RevisionID = @RevisionID
	AND IsPublised = 1;";

			SqlCommand command = new SqlCommand(sql, connection);
			command.Parameters.Add("@RevisionID", SqlDbType.UniqueIdentifier).Value = revisionID;

			connection.Open();
			SqlDataReader reader = command.ExecuteReader();

			reader.Read();
			webpage = CreateWebpageFromDataReader(reader);
			reader.Close();

			GetWebpageAreas(connection, webpage);

			connection.Close();

			return webpage;
        }

        public override Webpage GetPublishedWebpage(Guid webpageID)
        {
			// add main entry
			Webpage webpage = null;

			SqlConnection connection = new SqlConnection(this._connectionString);

			string sql = @"
SELECT 
	* 
FROM 
	purple_WebpageRevisions
WHERE
	WebpageID = @WebpageID
	AND IsPublised = 1;";

			SqlCommand command = new SqlCommand(sql, connection);
			command.Parameters.Add("@WebpageID", SqlDbType.UniqueIdentifier).Value = webpageID;

			connection.Open();
			SqlDataReader reader = command.ExecuteReader();

			reader.Read();
			webpage = CreateWebpageFromDataReader(reader);
			reader.Close();

			GetWebpageAreas(connection, webpage);

			connection.Close();

			return webpage;
        }

		private void GetWebpageAreas(SqlConnection connection, Webpage webpage)
		{
			string sql = @"
SELECT 
	* 
FROM 
	purple_WebpageAreas
WHERE
	RevisionID = @RevisionID;";

			SqlCommand command = new SqlCommand(sql, connection);
			command.Parameters.Add("@RevisionID", SqlDbType.UniqueIdentifier).Value = webpage.RevisionID;
			
			SqlDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				WebpageArea area = new WebpageArea();

				area.AreaID = new Guid(reader["AreaID"].ToString());
				area.WebpageID = webpage.WebpageID;
				area.RevisionID = webpage.RevisionID;
				area.SortOrder = (int)reader["SortOrder"];
				area.ContentPlaceHolderID = (string)reader["ContentPlaceHolderID"];
				area.ContentHtml = (string)reader["ContentHtml"];
				area.ControlName = (string)reader["ControlName"];

				webpage.Areas.Add(area);
			}
		}

		private Webpage CreateWebpageFromDataReader(SqlDataReader reader)
		{
			Webpage webpage = new Webpage();

			webpage.WebpageID = new Guid(reader["WebpageID"].ToString());
			webpage.ParentID = new Guid(reader["ParentID"].ToString());
			webpage.RevisionID = new Guid(reader["RevisionID"].ToString());
			webpage.Title = reader["Title"].ToString();
			webpage.MenuTitle = reader["MenuTitle"].ToString();
			webpage.Url = reader["Url"].ToString();
			webpage.Filename = reader["Filename"].ToString();
			webpage.MasterPageFilename = reader["MasterPageFilename"].ToString();
			webpage.MetaDescription = reader["MetaDescription"].ToString();
			webpage.MetaKeywords = reader["MetaKeywords"].ToString();
			webpage.ForceSsl = (bool)reader["ForceSsl"];
			webpage.ShowInMenu = (bool)reader["ShowInMenu"];
			webpage.MenuType = (MenuType)(int)reader["MenuType"];
			webpage.SortOrder = (int)reader["SortOrder"];
			webpage.FullUrl = (string)reader["FullUrl"];
			webpage.Editors = (string)reader["Editors"];

			webpage.RevisedByUsername = (string)reader["RevisedByUsername"];
			webpage.RevisedDate = (DateTime)reader["RevisedDate"];

			return webpage;
		}

        public override Webpage GetPublishedWebpage(string url)
        {
			// add main entry
			Webpage webpage = null;

			SqlConnection connection = new SqlConnection(this._connectionString);

			string sql = @"
SELECT 
	* 
FROM 
	purple_WebpageRevisions
WHERE
	Url = @Url
	AND IsPublished = 1;";

			SqlCommand command = new SqlCommand(sql, connection);
			command.Parameters.Add("@Url", SqlDbType.NVarChar).Value = url;

			connection.Open();
			SqlDataReader reader = command.ExecuteReader();

			reader.Read();
			webpage = CreateWebpageFromDataReader(reader);
			reader.Close();

			GetWebpageAreas(connection, webpage);

			connection.Close();

			return webpage;
        }

		public override List<Webpage> GetWebpageRevisions(Guid webpageID)
		{
			List<Webpage> revisions = new List<Webpage>();

			SqlConnection connection = new SqlConnection(this._connectionString);
			SqlConnection connection2 = new SqlConnection(this._connectionString);

			string sql = @"
SELECT 
	* 
FROM 
	purple_WebpageRevisions
WHERE
	WebpageID = @WebpageID
	AND IsPublised = 1;";

			SqlCommand command = new SqlCommand(sql, connection);
			command.Parameters.Add("@WebpageID", SqlDbType.UniqueIdentifier).Value = webpageID;

			connection.Open();
			connection2.Open();
			SqlDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				Webpage webpage = CreateWebpageFromDataReader(reader);
				GetWebpageAreas(connection2, webpage);

				revisions.Add(webpage);
			}
			reader.Close();
				
			connection.Close();
			connection2.Close();

			return revisions;
		}

		public override Webpage GetParentWebpage(Guid webpageID)
		{
			return GetPublishedWebpage(GetPublishedWebpage(webpageID).ParentID);
		}

		public override List<Webpage> GetChildWebpages(Guid webpageID)
		{
			throw new NotImplementedException();
		}

		public override List<UrlRedirect> GetUrlRedirects()
		{
			throw new NotImplementedException();
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

		public override List<CustomRouteEntry> GetCustomRouteEntries()
		{
			throw new NotImplementedException();
		}

		public override void AddCustomRouteEntry(CustomRouteEntry customRoute)
		{
			throw new NotImplementedException();
		}

		public override void UpdateCustomRouteEntry(CustomRouteEntry customRoute)
		{
			throw new NotImplementedException();
		}

		public override void DeleteCustomRouteEntry(string routeName)
		{
			throw new NotImplementedException();
		}

		public override UrlRedirect GetUrlRedirect(string fromUrl)
		{
			throw new NotImplementedException();
		}

		public override UrlRedirect GetUrlRedirect(Guid redirectID)
		{
			throw new NotImplementedException();
		}

		public override CustomRouteEntry GetCustomRouteEntry(string routeName)
		{
			throw new NotImplementedException();
		}
	}
}