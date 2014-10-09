using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Jhu.Graywulf.Schema;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Web.Security;
using Jhu.Graywulf.Keystone;

namespace Jhu.Graywulf.CasJobs
{
    /// <summary>
    /// Implements functions to create and use CasJobs MyDB as the user database
    /// in a Graywulf installation. Authentication relies on Keystone and no
    /// original CasJobs authentication is supported.
    /// </summary>
    public class CasJobsUserDatabaseFactory : UserDatabaseFactory
    {
        public CasJobsUserDatabaseFactory(Federation federation)
            :base(federation)
        {
        }

        public override void EnsureUserDatabaseExists(Registry.User user)
        {
            // This class is only used with tight Keystone integration.
            // We somehow have to find the token used to authenticate the user
            // because it is required to talk to the casjobs service in the name
            // of the user. The keystone user ID is conveyed in the thread's identity
            // but the authentication ticket is not available directly. 
            // The best we can do is to look up the token from the token
            // cache which should hold the last valid tokens indexed by user name.

            var ksclient = new KeystoneClient();

            // Get keystone user ID from identity and load user from keystone
            var keystoneID = GetKeystoneID(user);
            var ksuser = ksclient.GetUser(keystoneID);

            // Try to find a valid token in the cache
            Keystone.Token token;
            if (!KeystoneTokenCache.Instance.TryGetValueByUserName(ksuser.Name, ksuser.Name, out token))
            {
                throw new UnauthorizedAccessException("Keystone token required.");
            }

            // Now create a CasJobs client and look up or create user
            var cjclient = new CasJobsClient(ksclient);
            CasJobs.User cjuser = null;

            // Try to get the user from casjobs
            try
            {
                cjuser = cjclient.GetUser(keystoneID);

                // If the user exists we still have to make sure later that
                // mydb is created by executing a dummy query
            }
            catch
            {
                // This is a 404 which means the user doesn't exist. In this case
                // we have to create the user
            }

            if (cjuser == null)
            {
                try
                {
                    cjuser = CreateCasJobsUser(user, keystoneID);
                }
                catch
                {
                    // A CasJobs bug prevents retrieving user's that are in the database
                    // but have no MyDB assigned yet, so catch the exception here
                }
            }

            // Now check if MyDB exists.
            // TODO: there's a bug in casjobs which causes the user to be reported
            // non-existing when no MyDB has been created yet, but the user otherwise
            // exists in the database. Once the bug is fixed, this function will need
            // to be revised and tested.

            if (cjuser != null && cjuser.MyDBName == null)
            {
                // Here we need to delegate the user using its Keystone token
                // Submit a dummy job to force mydb creation
                cjclient.UserCredentials = new Keystone.KeystoneCredentials()
                {
                    TokenID = token.ID
                };

                var query = new Query()
                {
                    QueryText = "SELECT 1 AS a",
                    TaskName = "Dummy task to force MyDB creation."
                };

                cjclient.Submit("mydb", query);
            }
        }

        protected override DatasetBase OnGetUserDatabase(Registry.User user)
        {
            var keystoneID = GetKeystoneID(user);

            // CasJobs requires a keystone admin token to access REST services
            // TODO: this has to be moved to the keystone client and admin token
            // needs to be cached, because getting mydb is a frequent task

            var ksclient = new KeystoneClient();
            var cjclient = new CasJobsClient(ksclient);

            var cjuser = cjclient.GetUser(keystoneID);

            // Because the cas jobs web service doesn't expose the database password,
            // we need to look it up directly from batch admin

            string server, username, password, extra;

            using (var cn = new SqlConnection(CasJobsClient.Configuration.BatchAdminConnectionString))
            {
                cn.Open();

                var sql = "SELECT server, admin_usr, admin_pw, CStringExtra FROM MyDBHosts WHERE mydbhost = @mydbhost";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@mydbhost", SqlDbType.NVarChar).Value = cjuser.MyDBHost;

                    using (var dr = cmd.ExecuteReader())
                    {
                        dr.Read();

                        server = dr.GetString(0);
                        username = dr.GetString(1);
                        password = dr.GetString(2);
                        extra = dr.GetString(3);
                    }
                }
            }

            // Build connection string
            var csb = new SqlConnectionStringBuilder()
            {
                DataSource = server,
                InitialCatalog = cjuser.MyDBName,
                UserID = username,
                Password = password,
                PersistSecurityInfo = true,
                IntegratedSecurity = false,
            };

            var cstr = csb.ConnectionString + ';' + extra;

            var ds = new Jhu.Graywulf.Schema.SqlServer.SqlServerDataset()
            {
                Name = Jhu.Graywulf.Registry.Constants.MyDbName,
                ConnectionString = cstr,
                IsCacheable = false,
                IsMutable = true,
            };

            return ds;
        }

        private string GetKeystoneID(Registry.User user)
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

            return keystoneID;
        }

        private CasJobs.User CreateCasJobsUser(Registry.User user, string keystoneID)
        {
            var ksclient = new KeystoneClient();
            var cjclient = new CasJobsClient(ksclient);

            var cjuser = new User()
            {
                UserId = user.Name,
                Password = new Guid().ToString(),   // User 'random' password
                Email = user.Email,
                FullName = String.Format("{0} {1}", user.FirstName, user.LastName)
            };

            cjclient.Create(keystoneID, cjuser);

            return cjuser;
        }
    }
}