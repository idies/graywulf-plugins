using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Jhu.Graywulf.Schema;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Web.Security;
using Jhu.Graywulf.Keystone;

namespace Jhu.Graywulf.CasJobs
{
    public class CasJobsUserDatabaseFactory : UserDatabaseFactory
    {
        public CasJobsUserDatabaseFactory(Federation federation)
            :base(federation)
        {
        }

        public override void EnsureUserDatabaseExists(Registry.User user)
        {
            
        }

        public override DatasetBase GetUserDatabase(Registry.User user)
        {
            // Make sure user identities are loaded and find keystone user ID.
            user.Context = Federation.Context;
            user.LoadUserIdentities(false);

            string keystoneID = null;
            foreach (var id in user.UserIdentities.Values)
            {
                // Here we select the identity based on protocol name (should be Keystone)
                // This might not work when two different keystone services used but
                // that's a weird config anyway.
                if (StringComparer.InvariantCultureIgnoreCase.Compare(id.Protocol, Jhu.Graywulf.Web.Security.Constants.AuthorityNameKeystone) == 0)
                {
                    keystoneID = id.Identifier;
                    break;
                }
            }

            if (keystoneID == null)
            {
                throw new SecurityException("User Keystone ID not found");  // *** TODO
            }

            // CasJobs requires a keystone admin token to access REST services
            // TODO: this has to be moved to the keystone client and admin token
            // needs to be cached, because getting mydb is a frequent task

            var ksclient = new KeystoneClient();
            var cjclient = new CasJobsClient(ksclient);

            var cjuser = cjclient.GetUser(keystoneID);

            // Build connection string
            var csb = new SqlConnectionStringBuilder()
            {
                DataSource = cjuser.MyDBHost,
                InitialCatalog = cjuser.MyDBName
            };

            var config = CasJobsClient.Configuration;

            if (config.SqlUserName != null)
            {
                csb.UserID = config.SqlUserName;
                csb.Password = config.SqlPassword;
                csb.PersistSecurityInfo = true;
                csb.IntegratedSecurity = false;
            }
            else
            {
                csb.IntegratedSecurity = true;
            }

            var ds = new Jhu.Graywulf.Schema.SqlServer.SqlServerDataset()
            {
                Name = Jhu.Graywulf.Registry.Constants.MyDbName,
                ConnectionString = csb.ConnectionString,
                IsCacheable = false,
                IsMutable = true,
            };

            return ds;
        }
    }
}