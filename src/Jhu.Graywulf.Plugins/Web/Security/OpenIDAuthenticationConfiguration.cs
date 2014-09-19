using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Runtime.Serialization;

namespace Jhu.Graywulf.Web.Security
{
    public class OpenIDAuthenticationConfiguration : AuthenticationConfiguration
    {
        #region Static declarations

        private static ConfigurationPropertyCollection properties;

        private static readonly ConfigurationProperty propDiscoveryUri = new ConfigurationProperty(
            "discoveryUri", typeof(Uri), null, ConfigurationPropertyOptions.None);

        static OpenIDAuthenticationConfiguration()
        {
            properties = new ConfigurationPropertyCollection();

            properties.Add(propDiscoveryUri);
        }

        #endregion

        public Uri DiscoveryUri
        {
            get { return (Uri)base[propDiscoveryUri]; }
            set { base[propDiscoveryUri] = value; }
        }
    }
}
