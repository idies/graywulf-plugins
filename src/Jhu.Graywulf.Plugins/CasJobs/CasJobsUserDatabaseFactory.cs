using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Jhu.Graywulf.Check;
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
    public class CasJobsUserDatabaseFactory : UserDatabaseFactory, ICheckable
    {
        public CasJobsUserDatabaseFactory(Federation federation)
            : base(federation)
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

            var ip = new KeystoneIdentityProvider(Federation.Domain);
            var token = ip.GetCachedToken(user);

            // Now create a CasJobs client and look up or create user
            var ksclient = new KeystoneClient();
            var cjclient = new CasJobsClient(ksclient);
            CasJobs.User cjuser = null;

            // Try to get the user from casjobs
            try
            {
                cjuser = cjclient.GetUser(token.User.ID);

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
                    cjuser = CreateCasJobsUser(user, token.User.ID);
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

        protected override Jhu.Graywulf.Schema.SqlServer.SqlServerDataset OnGetUserDatabase(Registry.User user)
        {
            // CasJobs requires a keystone admin token to access REST services

            var ip = new KeystoneIdentityProvider(Federation.Domain);
            var token = ip.GetCachedToken(user);

            var ksclient = new KeystoneClient();
            var cjclient = new CasJobsClient(ksclient);

            var cjuser = cjclient.GetUser(token.User.ID);

            // Because the CasJobs web service doesn't expose the database password,
            // we need to look it up directly from batch admin

            string server, username, password, extra;

            using (var cn = new SqlConnection(CasJobsClient.Configuration.BatchAdminConnectionString))
            {
                cn.Open();

                // Use either mydbhost or server to find the right machine.
                var sql = "SELECT server, admin_usr, admin_pw, CStringExtra FROM MyDBHosts WHERE mydbhost = @server OR server = @server";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@server", SqlDbType.NVarChar).Value = cjuser.MyDBHost;

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
                Name = Jhu.Graywulf.Registry.Constants.UserDbName,
                ConnectionString = cstr,
                IsCacheable = false,
                IsMutable = true,
            };

            return ds;
        }

        protected override ServerInstance OnGetUserDatabaseServerInstance(Registry.User user)
        {
            var ds = OnGetUserDatabase(user);
            return GetAssociatedServerInstance(ds);
        }

        private ServerInstance GetAssociatedServerInstance(Jhu.Graywulf.Schema.SqlServer.SqlServerDataset dataset)
        {
            // This function is currently unused

            var hostname = dataset.HostName;
            var instancename = dataset.InstanceName;

            // Find the server instance based on Graywulf settings
            var mr = Federation.UserDatabaseVersion.ServerVersion.MachineRole;

            // Find the right machine
            mr.LoadMachines(false);
            var m = (from mi in mr.Machines.Values
                     where Registry.Entity.StringComparer.Compare(mi.HostName.Value, hostname) == 0
                     select mi)
                    .FirstOrDefault();

            if (m == null)
            {
                return null;
            }

            // Find the right server instance
            m.LoadServerInstances(false);
            var si = (from sii in m.ServerInstances.Values
                      where Registry.Entity.StringComparer.Compare(sii.InstanceName, instancename) == 0
                      select sii)
                     .FirstOrDefault();

            return si;
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

        #region Check routines

        public override IEnumerable<CheckRoutineBase> GetCheckRoutines()
        {
            yield return new CasJobsCheck();
        }

        #endregion
    }
}