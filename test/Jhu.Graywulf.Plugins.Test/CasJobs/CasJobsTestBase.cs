using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.Test;
using Jhu.Graywulf.Keystone;

namespace Jhu.Graywulf.CasJobs
{
    public class CasJobsTestBase : TestClassBase
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
                    client = new CasJobsClient(KeystoneClient);
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
                    keystoneClient = new KeystoneClient();
                }

                return keystoneClient;
            }
        }
    }
}
