using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Jhu.Graywulf.Logging
{
    public class SciServerLogWriterConfiguration : LogWriterConfigurationBase
    {
        #region Static declarations

        private static ConfigurationPropertyCollection properties;

        private static readonly ConfigurationProperty propApplicationName = new ConfigurationProperty(
            "applicationName", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty propMessagingHost = new ConfigurationProperty(
            "messagingHost", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty propExchangeName = new ConfigurationProperty(
            "exchangeName", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty propDatabaseQueueName = new ConfigurationProperty(
            "databaseQueueName", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

        static SciServerLogWriterConfiguration()
        {
            properties = new ConfigurationPropertyCollection();

            properties.Add(propApplicationName);
            properties.Add(propMessagingHost);
            properties.Add(propExchangeName);
            properties.Add(propDatabaseQueueName);
        }

        #endregion
        #region Properties

        [ConfigurationProperty("applicationName", DefaultValue = "Graywulf")]
        public string ApplicationName
        {
            get { return (string)base[propApplicationName]; }
            set { base[propApplicationName] = value; }
        }

        [ConfigurationProperty("messagingHost")]
        public string MessagingHost
        {
            get { return (string)base[propMessagingHost]; }
            set { base[propMessagingHost] = value; }
        }

        [ConfigurationProperty("exchangeName")]
        public string ExchangeName
        {
            get { return (string)base[propExchangeName]; }
            set { base[propExchangeName] = value; }
        }

        [ConfigurationProperty("databaseQueueName")]
        public string DatabaseQueueName
        {
            get { return (string)base[propDatabaseQueueName]; }
            set { base[propDatabaseQueueName] = value; }
        }

        #endregion

        protected override LogWriterBase OnCreateLogWriter()
        {
            return new SciServerLogWriter()
            {
                ApplicationName = ApplicationName,
                MessagingHost = MessagingHost,
                ExchangeName = ExchangeName,
                DatabaseQueueName = DatabaseQueueName,
            };
        }
    }
}
