
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Purple.Core
{
    public class PurpleConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection DataProviders
        {
            get
            {
                return (ProviderSettingsCollection)base["providers"];
            }
        }

        [ConfigurationProperty("defaultDataProvider", DefaultValue = "XmlProvider")]
        public string DefaultDataProvider
        {
            get
            {
				return (string)base["defaultDataProvider"];
            }
            set
            {
				base["defaultDataProvider"] = value;
            }
        }

		[ConfigurationProperty("pageSuffix", DefaultValue = "")]
		public string PageSuffix
		{
			get
			{
				return (string)base["pageSuffix"];
			}
			set
			{
				base["pageSuffix"] = value;
			}
		}

		[ConfigurationProperty("defaultMasterPageFilename", DefaultValue = "Default.Master")]
		public string DefaultMasterPageFilename
		{
			get
			{
				return (string)base["defaultMasterPageFilename"];
			}
			set
			{
				base["defaultMasterPageFilename"] = value;
			}
		}


		[ConfigurationProperty("baseUrl", IsRequired=true)]
		public string BaseUrl
		{
			get
			{
				return (string)base["baseUrl"];
			}
			set
			{
				base["baseUrl"] = value;
			}
		}


		[ConfigurationProperty("secureUrl")]
		public string SecureUrl
		{
			get
			{
				return (string)base["secureUrl"];
			}
			set
			{
				base["secureUrl"] = value;
			}
		}

		[ConfigurationProperty("cmsPath", DefaultValue = "~/cms/")]
		public string CmsPath
		{
			get
			{
				return (string)base["cmsPath"];
			}
			set
			{
				base["cmsPath"] = value;
			}
		}

		[ConfigurationProperty("roleSuperAdmin", DefaultValue = "purple-super-admin")]
		public string RoleSuperAdmin
		{
			get
			{
				return (string)base["roleSuperAdmin"];
			}
			set
			{
				base["roleSuperAdmin"] = value;
			}
		}

		[ConfigurationProperty("roleEditor", DefaultValue = "purple-editor")]
		public string RoleEditor
		{
			get
			{
				return (string)base["roleEditor"];
			}
			set
			{
				base["roleEditor"] = value;
			}
		}

		[ConfigurationProperty("trailingSlash", DefaultValue = "false")]
		public bool TrailingSlash
		{
			get
			{
				return (bool)base["trailingSlash"];
			}
			set
			{
				base["trailingSlash"] = value;
			}
		}
    }
}