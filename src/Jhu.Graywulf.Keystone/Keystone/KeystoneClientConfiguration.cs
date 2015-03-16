using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Runtime.Serialization;

namespace Jhu.Graywulf.Keystone
{
    public class KeystoneClientConfiguration : ConfigurationSection
    {
        #region Static declarations

        private static ConfigurationPropertyCollection properties;

        private static readonly ConfigurationProperty propAuthorityName = new ConfigurationProperty(
            "authorityName", typeof(string), Constants.KeystoneAuthorityName, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propBaseUri = new ConfigurationProperty(
            "baseUri", typeof(Uri), new Uri(Constants.KeystoneDefaultUri), ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty propDomain = new ConfigurationProperty(
            "domain", typeof(string), Constants.KeystoneDefaultDomain, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propAdminToken = new ConfigurationProperty(
            "adminToken", typeof(string), null, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propAdminProject = new ConfigurationProperty(
            "adminProject", typeof(string), null, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propAdminUserName = new ConfigurationProperty(
            "adminUserName", typeof(string), null, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propAdminPassword = new ConfigurationProperty(
            "adminPassword", typeof(string), null, ConfigurationPropertyOptions.None);

        static KeystoneClientConfiguration()
        {
            properties = new ConfigurationPropertyCollection();

            properties.Add(propAuthorityName);
            properties.Add(propBaseUri);
            properties.Add(propDomain);
            properties.Add(propAdminToken);
            properties.Add(propAdminProject);
            properties.Add(propAdminUserName);
            properties.Add(propAdminPassword);
        }

        #endregion
        #region Properties

        [ConfigurationProperty("authorityName")]
        public string AuthorityName
        {
            get { return (string)base[propAuthorityName]; }
            set { base[propAuthorityName] = value; }
        }

        /// <summary>
        /// Gets or sets the base URL of the Keystone service
        /// </summary>
        [ConfigurationProperty("baseUri")]
        public Uri BaseUri
        {
            get { return (Uri)base[propBaseUri]; }
            set { base[propBaseUri] = value; }
        }


        /// <summary>
        /// Gets or sets the Keystone domain associated with the
        /// Graywulf domain
        /// </summary>
        [ConfigurationProperty("domain", DefaultValue = Constants.KeystoneDefaultDomain)]
        public string Domain
        {
            get { return (string)base[propDomain]; }
            set { base[propDomain] = value; }
        }

        /// <summary>
        /// Gets or sets the token identifying the administrator
        /// of the Keystone service.
        /// </summary>
        [ConfigurationProperty("adminToken")]
        public string AdminToken
        {
            get { return (string)base[propAdminToken]; }
            set { base[propAdminToken] = value; }
        }

        /// <summary>
        /// Gets or sets the project (tenant) name of
        /// the administrator.
        /// </summary>
        [ConfigurationProperty("adminProject")]
        public string AdminProject
        {
            get { return (string)base[propAdminProject]; }
            set { base[propAdminProject] = value; }
        }

        /// <summary>
        /// Gets or sets the user name of the identity used to
        /// manage the keystone service.
        /// </summary>
        [ConfigurationProperty("adminUserName")]
        public string AdminUserName
        {
            get { return (string)base[propAdminUserName]; }
            set { base[propAdminUserName] = value; }
        }

        /// <summary>
        /// Gets or sets the password of the identity used to
        /// manage the keystone instance.
        /// </summary>
        [ConfigurationProperty("adminPassword")]
        public string AdminPassword
        {
            get { return (string)base[propAdminPassword]; }
            set { base[propAdminPassword] = value; }
        }

        #endregion
        #region Constructors and initializers

        public KeystoneClientConfiguration()
        {
        }

        #endregion

        internal KeystoneCredentials GetAdminCredentials()
        {
            return new Keystone.KeystoneCredentials()
            {
                TokenID = AdminToken,
                DomainID = Domain,
                ProjectName = AdminProject,
                UserName = AdminUserName,
                Password = AdminPassword,
            };
        }
    }
}
