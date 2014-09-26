using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Jhu.Graywulf.Web.Security
{
    public class OpenIDConfiguration : ConfigurationSection
    {
        #region Static declarations

        private static ConfigurationPropertyCollection properties;

        private static readonly ConfigurationProperty propOpenIDProviders = new ConfigurationProperty(
            null, typeof(OpenIDProviderSettingsCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        static OpenIDConfiguration()
        {
            properties = new ConfigurationPropertyCollection();

            properties.Add(propOpenIDProviders);
            
        }

        #endregion

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }

        [ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public OpenIDProviderSettingsCollection OpenIDProviders
        {
            get { return (OpenIDProviderSettingsCollection)base[propOpenIDProviders]; }
            set { base[propOpenIDProviders] = value; }
        }
    }
}
