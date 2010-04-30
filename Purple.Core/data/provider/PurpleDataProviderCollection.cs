using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;

namespace Purple.Core
{
    public class PurpleDataProviderCollection : ProviderCollection
    {
        // Return an instance of DataProvider
        // for a specified provider name
        new public PurpleDataProvider this[string name]
        {
            get { return (PurpleDataProvider)base[name]; }
        }
    }
}