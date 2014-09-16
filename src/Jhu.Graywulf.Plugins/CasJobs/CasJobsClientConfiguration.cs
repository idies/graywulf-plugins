using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Runtime.Serialization;


namespace Jhu.Graywulf.CasJobs
{
    public class CasJobsClientConfiguration : ConfigurationSection
    {
        #region Static declarations

        private static ConfigurationPropertyCollection properties;

        private static readonly ConfigurationProperty propBaseUri = new ConfigurationProperty(
            "baseUri", typeof(Uri), null, ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty propSqlUserName = new ConfigurationProperty(
            "sqlUserName", typeof(string), null, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propSqlPassword = new ConfigurationProperty(
            "sqlPassword", typeof(string), null, ConfigurationPropertyOptions.None);

        static CasJobsClientConfiguration()
        {
            properties = new ConfigurationPropertyCollection();

            properties.Add(propBaseUri);
            properties.Add(propSqlUserName);
            properties.Add(propSqlPassword);
        }

        #endregion
        #region Properties

        [ConfigurationProperty("baseUri")]
        public Uri BaseUri
        {
            get { return (Uri)base[propBaseUri]; }
            set { base[propBaseUri] = value; }
        }

        [ConfigurationProperty("sqlUserName")]
        public string SqlUserName
        {
            get { return (string)base[propSqlUserName]; }
            set { base[propSqlUserName] = value; }
        }

        [ConfigurationProperty("sqlPassword")]
        public string SqlPassword
        {
            get { return (string)base[propSqlPassword]; }
            set { base[propSqlPassword] = value; }
        }

        #endregion
        #region Constructors and initializers

        public CasJobsClientConfiguration()
        {
        }

        #endregion
    }
}
