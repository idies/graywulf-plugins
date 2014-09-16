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
                    // TODO: modify once keystone settings is moved to own config section
                    client = new CasJobsClient(new Uri(CasJobs.AppSettings.Url));
                    client.AdminCredentials = new KeystoneCredentials()
                    {
                        TokenID = Keystone.AppSettings.AdminToken
                    };
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
                    // TODO: modify once keystone settings is moved to own config section
                    keystoneClient = new KeystoneClient(new Uri(Keystone.AppSettings.Url));
                    keystoneClient.AdminCredentials = new KeystoneCredentials()
                    {
                        TokenID = Keystone.AppSettings.AdminToken
                    };
                }

                return keystoneClient;
            }
        }
    }
}
