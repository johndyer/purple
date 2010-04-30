using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Purple.Core
{
	public class PurpleSettings
	{
		private static PurpleConfiguration _purpleConfiguration = null;

		public static string PageSuffix
		{
			get
			{
				if (_purpleConfiguration == null) 
					GetConfiguration();
				return _purpleConfiguration.PageSuffix;
			}
		}

		public static string DefaultMasterPageFilename
		{
			get
			{
				if (_purpleConfiguration == null)
					GetConfiguration();
				return _purpleConfiguration.DefaultMasterPageFilename;
			}
		}

		public static string BaseUrl
		{
			get
			{
				if (_purpleConfiguration == null)
					GetConfiguration();
				return _purpleConfiguration.BaseUrl;
			}
		}

		public static string SecureUrl
		{
			get
			{
				if (_purpleConfiguration == null)
					GetConfiguration();
				return _purpleConfiguration.SecureUrl;
			}
		}

		public static string CmsPath
		{
			get
			{
				if (_purpleConfiguration == null)
					GetConfiguration();
				return _purpleConfiguration.CmsPath;
			}
		}

		public static string RoleSuperAdmin
		{
			get
			{
				if (_purpleConfiguration == null)
					GetConfiguration();
				return _purpleConfiguration.RoleSuperAdmin;
			}
		}

		public static string RoleEditor
		{
			get
			{
				if (_purpleConfiguration == null)
					GetConfiguration();
				return _purpleConfiguration.RoleEditor;
			}
		}

		public static bool TrailingSlash
		{
			get
			{
				if (_purpleConfiguration == null)
					GetConfiguration();
				return _purpleConfiguration.TrailingSlash;
			}
		}


		private static void GetConfiguration()
		{
			_purpleConfiguration = (PurpleConfiguration)ConfigurationManager.GetSection("PurpleConfiguration");
		}

	}
}
