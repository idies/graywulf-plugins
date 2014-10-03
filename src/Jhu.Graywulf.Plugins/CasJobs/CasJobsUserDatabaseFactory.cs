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
#if false
            User cjuser = null;

            var keystoneID = GetKeystoneID(user);

            var ksclient = new KeystoneClient();
            var cjclient = new CasJobsClient(ksclient);

            // Try to get the user from casjobs
            try
            {
                cjuser = cjclient.GetUser(keystoneID);

                // If the user exists that means their MYDB exists as well
                // nothing to do here
            }
            catch
            {
            }

            /*
            

            var user = new User()
            {
                UserId = ksuser.Name,
                Password = "alma",
                Email = ksuser.Email,
                FullName = !String.IsNullOrWhiteSpace(ksuser.Description) ? ksuser.Description : ksuser.Name,
            };

            Client.Create(ksuser.ID, user);

            // Now submit a dummy job to force mydb creation

            Client.UserCredentials = new Keystone.KeystoneCredentials()
            {
                TokenID = KeystoneClient.Authenticate("default", "test1", "test1", "alma").ID
            };

            var query = new Query()
            {
                QueryText = "SELECT 1 AS a",
                TaskName = "Dummy task to force MyDB creation."
            };

            Client.Submit("mydb", query);
             * */
#endif
        }

        public override DatasetBase GetUserDatabase(Registry.User user)
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

                // Filter on the server name because that's what returned by the web service
                var sql = "SELECT server, admin_usr, admin_pw, CStringExtra FROM MyDBHosts WHERE server = @mydbhost";

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
    }
}