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

        private static readonly ConfigurationProperty propBatchAdminConnectionString = new ConfigurationProperty(
            "batchAdminConnectionString", typeof(string), null, ConfigurationPropertyOptions.None);

        static CasJobsClientConfiguration()
        {
            properties = new ConfigurationPropertyCollection();

            properties.Add(propBaseUri);
            properties.Add(propBatchAdminConnectionString);
        }

        #endregion
        #region Properties

        [ConfigurationProperty("baseUri")]
        public Uri BaseUri
        {
            get { return (Uri)base[propBaseUri]; }
            set { base[propBaseUri] = value; }
        }

        [ConfigurationProperty("batchAdminConnectionString")]
        public string BatchAdminConnectionString
        {
            get { return (string)base[propBatchAdminConnectionString]; }
            set { base[propBatchAdminConnectionString] = value; }
        }

        #endregion
        #region Constructors and initializers

        public CasJobsClientConfiguration()
        {
        }

        #endregion
    }
}
