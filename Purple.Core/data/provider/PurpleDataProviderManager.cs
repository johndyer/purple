using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web.Configuration;

namespace Purple.Core
{
    public class PurpleDataProviderManager
    {
        private static PurpleDataProvider defaultProvider;
        private static PurpleDataProviderCollection providers;

        static PurpleDataProviderManager()
        {
            Initialize();
        }

        private static void Initialize()
        {
            PurpleConfiguration configuration = (PurpleConfiguration)ConfigurationManager.GetSection("PurpleConfiguration");

            if (configuration == null)
                throw new ConfigurationErrorsException("SampleProvider configuration section is not set correctly.");

            providers = new PurpleDataProviderCollection();

            ProvidersHelper.InstantiateProviders(configuration.DataProviders, providers, typeof(PurpleDataProvider));

            providers.SetReadOnly();

            defaultProvider = providers[configuration.DefaultDataProvider];

            if (defaultProvider == null)
                throw new Exception("defaultProvider");
        }

        public static PurpleDataProvider Provider
        {
            get
            {
                return defaultProvider;
            }
        }

        public static PurpleDataProviderCollection Providers
        {
            get
            {
                return providers;
            }
        }
    }
}