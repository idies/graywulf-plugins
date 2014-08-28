using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Jhu.Graywulf.CasJobs
{
    public static class AppSettings
    {
        private static string GetValue(string key)
        {
            var section = (NameValueCollection)ConfigurationManager.GetSection("Jhu.Graywulf/CasJobs");
            if (section != null)
            {
                return (string)section[key];
            }
            else
            {
                return null;
            }
        }
        
        public static string Url
        {
            get { return GetValue("Url"); }
        }
    }
}
