using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Jhu.Graywulf.Web.Security
{
    [ConfigurationCollection(typeof(OpenIDProviderSettings))]
    public class OpenIDProviderSettingsCollection : ConfigurationElementCollection
    {
        #region Static declarations

        private static ConfigurationPropertyCollection properties;

        static OpenIDProviderSettingsCollection()
        {
            properties = new ConfigurationPropertyCollection();
        }

        #endregion

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }

        public OpenIDProviderSettings this[int index]
        {
            get
            {
                return (OpenIDProviderSettings)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public OpenIDProviderSettings this[string name]
        {
            get
            {
                return (OpenIDProviderSettings)BaseGet(name);
            }
        }

        public int IndexOf(OpenIDProviderSettings settings)
        {
            return BaseIndexOf(settings);
        }

        // the connection string behavior is strange in that is acts kind of like a
        // basic map and partially like a add remove clear collection
        // Overriding these methods allows for the specific behaviors to be
        // patterened
        protected override void BaseAdd(int index, ConfigurationElement element)
        {
            if (index == -1)
            {
                BaseAdd(element, false);
            }
            else
            {
                base.BaseAdd(index, element);
            }
        }

        public void Add(OpenIDProviderSettings settings)
        {
            BaseAdd(settings);
        }

        public void Remove(OpenIDProviderSettings settings)
        {
            if (BaseIndexOf(settings) >= 0)
            {
                BaseRemove(settings.AuthorityUri.ToString());
            }
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new OpenIDProviderSettings();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((OpenIDProviderSettings)element).AuthorityUri.ToString();
        }

        public void Clear()
        {
            BaseClear();
        }

        public OpenIDProviderSettingsCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

    }
}
