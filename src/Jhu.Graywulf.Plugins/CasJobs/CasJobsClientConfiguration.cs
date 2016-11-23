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

        private static readonly ConfigurationProperty propDefaultSchema = new ConfigurationProperty(
            "defaultSchema", typeof(string), null, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propScratchDbServer = new ConfigurationProperty(
            "scratchDbServer", typeof(string), null, ConfigurationPropertyOptions.None);

        static CasJobsClientConfiguration()
        {
            properties = new ConfigurationPropertyCollection();

            properties.Add(propBaseUri);
            properties.Add(propBatchAdminConnectionString);
            properties.Add(propDefaultSchema);
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

        [ConfigurationProperty("defaultSchema")]
        public string DefaultSchema
        {
            get { return (string)base[propDefaultSchema]; }
            set { base[propDefaultSchema] = value; }
        }

        [ConfigurationProperty("scratchDbServer")]
        public string ScratchDbServer
        {
            get { return (string)base[propScratchDbServer]; }
            set { base[propScratchDbServer] = value; }
        }

        #endregion
        #region Constructors and initializers

        public CasJobsClientConfiguration()
        {
        }

        #endregion
    }
}
