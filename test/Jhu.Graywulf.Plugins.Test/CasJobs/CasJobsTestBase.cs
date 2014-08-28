using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.Keystone;

namespace Jhu.Graywulf.CasJobs
{
    public class CasJobsTestBase
    {
        private CasJobsClient client;
        private KeystoneClient keystoneClient;

        protected CasJobsClient Client
        {
            get
            {
                if (client == null)
                {
                    client = new CasJobsClient(new Uri(CasJobs.AppSettings.Url));
                    client.AdminAuthToken = Keystone.AppSettings.AdminToken;
                }

                return client;
            }
        }

        protected KeystoneClient KeystoneClient
        {
            get
            {
                if (keystoneClient == null)
                {
                    keystoneClient = new KeystoneClient(new Uri(Keystone.AppSettings.Url));
                    keystoneClient.AdminAuthToken = Keystone.AppSettings.AdminToken;
                }

                return keystoneClient;
            }
        }
    }
}
