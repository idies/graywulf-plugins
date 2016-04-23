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

        private static readonly ConfigurationProperty propIsEnabled = new ConfigurationProperty(
            "enabled", typeof(bool), false, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propIsMasterAuthority = new ConfigurationProperty(
            "isMasterAuthority", typeof(bool), false, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propAuthTokenParameter = new ConfigurationProperty(
            "authTokenParameter", typeof(string), Constants.KeystoneDefaultAuthTokenParameter, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propAuthTokenHeader = new ConfigurationProperty(
            "authTokenHeader", typeof(string), Constants.KeystoneDefaultAuthTokenHeader, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propAuthTokenCookie = new ConfigurationProperty(
            "authTokenCookie", typeof(string), Constants.KeystoneDefaultAuthTokenCookie, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propTokenRenewalInterval = new ConfigurationProperty(
            "tokenRenewalInterval", typeof(int), Constants.KeystoneTokenRenewalInterval, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propDefaultRole = new ConfigurationProperty(
            "defaultRole", typeof(bool), Constants.KeystoneDefaultRole, ConfigurationPropertyOptions.None);

        static KeystoneAuthenticationConfiguration()
        {
            properties = new ConfigurationPropertyCollection();

            properties.Add(propIsEnabled);
            properties.Add(propIsMasterAuthority);
            properties.Add(propAuthTokenParameter);
            properties.Add(propAuthTokenHeader);
            properties.Add(propAuthTokenCookie);
            properties.Add(propTokenRenewalInterval);
            properties.Add(propDefaultRole);
        }

        #endregion
        #region Properties

        [ConfigurationProperty("enabled", DefaultValue = false)]
        public bool IsEnabled
        {
            get { return (bool)base[propIsEnabled]; }
            set { base[propIsEnabled] = value; }
        }

        [ConfigurationProperty("isMasterAuthority")]
        public bool IsMasterAuthority
        {
            get { return (bool)base[propIsMasterAuthority]; }
            set { base[propIsMasterAuthority] = value; }
        }

        /// <summary>
        /// Gets or sets the base URL of the Keystone service
        /// </summary>
        [ConfigurationProperty("authTokenParameter", DefaultValue = Constants.KeystoneDefaultAuthTokenParameter)]
        public string AuthTokenParameter
        {
            get { return (string)base[propAuthTokenParameter]; }
            set { base[propAuthTokenParameter] = value; }
        }

        [ConfigurationProperty("authTokenHeader", DefaultValue = Constants.KeystoneDefaultAuthTokenHeader)]
        public string AuthTokenHeader
        {
            get { return (string)base[propAuthTokenHeader]; }
            set { base[propAuthTokenHeader] = value; }
        }

        [ConfigurationProperty("authTokenCookie", DefaultValue = Constants.KeystoneDefaultAuthTokenCookie)]
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

        [ConfigurationProperty("defaultRole", DefaultValue = Constants.KeystoneDefaultRole)]
        public string DefaultRole
        {
            get { return (string)base[propDefaultRole]; }
            set { base[propDefaultRole] = value; }
        }

        #endregion
    }
}
