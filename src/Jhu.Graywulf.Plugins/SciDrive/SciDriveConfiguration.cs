using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Runtime.Serialization;


namespace Jhu.Graywulf.SciDrive
{
    public class SciDriveConfiguration : ConfigurationSection
    {
        #region Static declarations

        private static ConfigurationPropertyCollection properties;

        private static readonly ConfigurationProperty propBaseUri = new ConfigurationProperty(
            "baseUri", typeof(Uri), null, ConfigurationPropertyOptions.IsRequired);

        private static readonly ConfigurationProperty propIsTrustEnabled = new ConfigurationProperty(
            "isTrustEnabled", typeof(bool), false, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propTrustRole = new ConfigurationProperty(
            "trustRole", typeof(string), null, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty propTrustExpiresAfter = new ConfigurationProperty(
            "trustExpiresAfter", typeof(TimeSpan), TimeSpan.Zero, ConfigurationPropertyOptions.None);

        static SciDriveConfiguration()
        {
            properties = new ConfigurationPropertyCollection();

            properties.Add(propBaseUri);
            properties.Add(propIsTrustEnabled);
            properties.Add(propTrustRole);
            properties.Add(propTrustExpiresAfter);
        }

        #endregion
        #region Properties

        [ConfigurationProperty("baseUri")]
        public Uri BaseUri
        {
            get { return (Uri)base[propBaseUri]; }
            set { base[propBaseUri] = value; }
        }

        [ConfigurationProperty("isTrustEnabled")]
        public bool IsTrustEnabled
        {
            get { return (bool)base[propIsTrustEnabled]; }
            set { base[propIsTrustEnabled] = value; }
        }

        [ConfigurationProperty("trustRole")]
        public string TrustRole
        {
            get { return (string)base[propTrustRole]; }
            set { base[propTrustRole] = value; }
        }

        [ConfigurationProperty("trustExpiresAfter")]
        public TimeSpan TrustExpiresAfter
        {
            get { return (TimeSpan)base[propTrustExpiresAfter]; }
            set { base[propTrustExpiresAfter] = value; }
        }

        #endregion
        #region Constructors and initializers

        public SciDriveConfiguration()
        {
        }

        #endregion
    }
}
