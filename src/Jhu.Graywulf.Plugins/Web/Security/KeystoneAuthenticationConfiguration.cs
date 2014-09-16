using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Runtime.Serialization;

namespace Jhu.Graywulf.Web.Security
{
    public class KeystoneAuthenticationConfiguration : ConfigurationSection
    {
        #region Static declarations

        private static ConfigurationPropertyCollection properties;

        private static readonly ConfigurationProperty propEnabled = new ConfigurationProperty(
            "enabled", typeof(bool), false, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propAuthTokenParameter = new ConfigurationProperty(
            "authTokenParameter", typeof(string), null, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propAuthTokenHeader = new ConfigurationProperty(
            "authTokenHeader", typeof(string), null, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propAuthTokenCookie = new ConfigurationProperty(
            "authTokenCookie", typeof(string), null, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propTokenRenewalInterval = new ConfigurationProperty(
            "tokenRenewalInterval", typeof(int), null, ConfigurationPropertyOptions.None);

        static KeystoneAuthenticationConfiguration()
        {
            properties = new ConfigurationPropertyCollection();

            properties.Add(propEnabled);
            properties.Add(propAuthTokenParameter);
            properties.Add(propAuthTokenHeader);
            properties.Add(propAuthTokenCookie);
            properties.Add(propTokenRenewalInterval);
        }

        #endregion
        #region Properties

        [ConfigurationProperty("enabled", DefaultValue=false)]
        public bool Enabled
        {
            get { return (bool)base[propEnabled]; }
            set { base[propEnabled] = value; }
        }

        /// <summary>
        /// Gets or sets the base URL of the Keystone service
        /// </summary>
        [ConfigurationProperty("authTokenParameter")]
        public string AuthTokenParameter
        {
            get { return (string)base[propAuthTokenParameter]; }
            set { base[propAuthTokenParameter] = value; }
        }

        [ConfigurationProperty("authTokenHeader")]
        public string AuthTokenHeader
        {
            get { return (string)base[propAuthTokenHeader]; }
            set { base[propAuthTokenHeader] = value; }
        }

        [ConfigurationProperty("authTokenCookie")]
        public string AuthTokenCookie
        {
            get { return (string)base[propAuthTokenCookie]; }
            set { base[propAuthTokenCookie] = value; }
        }

        [ConfigurationProperty("tokenRenewalInterval", DefaultValue = Constants.KeystoneTokenRenewalInterval)]
        public int TokenRenewalInterval
        {
            get { return (int)base[propTokenRenewalInterval]; }
            set { base[propTokenRenewalInterval] = value; }
        }

        #endregion
    }
}
