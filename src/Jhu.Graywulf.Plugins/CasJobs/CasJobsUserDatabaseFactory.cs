using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Jhu.Graywulf.Check;
using Jhu.Graywulf.Schema;
using Jhu.Graywulf.Schema.SqlServer;
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
        public CasJobsUserDatabaseFactory(FederationContext context)
            : base(context)
        {
        }

        protected override void EnsureUserDatabaseExists(Registry.User user, SqlServerDataset dataset)
        {
            // TODO: add scratch DB testing, though we cannot create that on the fly

            if (SchemaManager.Comparer.Compare(dataset.Name, Registry.Constants.UserDbName) == 0)
            {
                EnsureMyDbExists(user);
            }
        }

        private void EnsureMyDbExists(Registry.User user)
        {
            // This class is only used with tight Keystone integration.
            // We somehow have to find the token used to authenticate the user
            // because it is required to talk to the casjobs service in the name
            // of the user. The keystone user ID is conveyed in the thread's identity
            // but the authentication ticket is not available directly. 
            // The best we can do is to look up the token from the token
            // cache which should hold the last valid tokens indexed by user name.

            var ip = new KeystoneIdentityProvider(FederationContext.Domain);
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

        protected override void EnsureUserDatabaseConfigured(Registry.User user, SqlServerDataset dataset)
        {
            // Make sure the default schema exists

            using (var cn = (SqlConnection)dataset.OpenConnection())
            {
                var sql =
@"IF SCHEMA_ID('{0}') IS NULL
EXEC('CREATE SCHEMA [{0}]')
";

                sql = String.Format(sql, dataset.DefaultSchemaName);

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected override Dictionary<string, SqlServerDataset> OnGetUserDatabases(Registry.User user)
        {
            var cjuser = GetCasJobsUser(user);

            var mydb = GetMyDB(cjuser);
            var scratchdb = GetScratchDb(cjuser);
            var res = new Dictionary<string, SqlServerDataset>(SchemaManager.Comparer);

            if (mydb != null)
            {
                res.Add(mydb.Name, mydb);
            }

            if (scratchdb != null)
            {
                res.Add(scratchdb.Name, scratchdb);
            }

            return res;
        }

        private CasJobs.User GetCasJobsUser(Registry.User user)
        {
            // CasJobs requires a keystone admin token to access REST services
            var ip = new KeystoneIdentityProvider(FederationContext.Domain);
            var token = ip.GetCachedToken(user);
            var ksclient = new KeystoneClient();
            var cjclient = new CasJobsClient(ksclient);
            var cjuser = cjclient.GetUser(token.User.ID);

            return cjuser;
        }

        private SqlServerDataset GetMyDB(CasJobs.User cjuser)
        {
            // Because the CasJobs web service doesn't expose the database password,
            // we need to look it up directly from batch admin

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
                        if (dr.Read())
                        {
                            var csb = new SqlConnectionStringBuilder()
                            {
                                DataSource = dr.GetString(0),
                                InitialCatalog = cjuser.MyDBName,
                                UserID = dr.GetString(1),
                                Password = dr.GetString(2),
                                PersistSecurityInfo = true,
                                IntegratedSecurity = false,
                            };

                            return new SqlServerDataset()
                            {
                                Name = Registry.Constants.UserDbName,
                                DefaultSchemaName = CasJobsClient.Configuration.DefaultSchema ?? Schema.SqlServer.Constants.DefaultSchemaName,
                                ConnectionString = csb.ConnectionString,
                                IsCacheable = false,
                                IsMutable = true,
                                IsRestrictedSchema = false,
                            };
                        }
                        else
                        {
                            throw new CasJobsException("MyDB cannot be found.");
                        }
                    }
                }
            }
        }

        private SqlServerDataset GetScratchDb(CasJobs.User cjuser)
        {
            if (String.IsNullOrWhiteSpace(CasJobsClient.Configuration.ScratchDbServer))
            {
                return null;
            }

            using (var cn = new SqlConnection(CasJobsClient.Configuration.BatchAdminConnectionString))
            {
                cn.Open();

                var sql = "SELECT Host, Cat, Usr, Password FROM batch.Servers WHERE Name = @name";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = CasJobsClient.Configuration.ScratchDbServer;

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            var csb = new SqlConnectionStringBuilder()
                            {
                                DataSource = dr.GetString(0),
                                InitialCatalog = dr.GetString(1),
                                UserID = dr.GetString(2),
                                Password = dr.GetString(3),
                                PersistSecurityInfo = true,
                                IntegratedSecurity = false,
                            };

                            return new SqlServerDataset()
                            {
                                Name = Constants.ScratchDbName,
                                DefaultSchemaName = "WSID_" + cjuser.WebServicesId,
                                ConnectionString = csb.ConnectionString,
                                IsCacheable = false,
                                IsMutable = true,
                                IsRestrictedSchema = true,
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        protected override Dictionary<string, ServerInstance> OnGetUserDatabaseServerInstances(Registry.User user)
        {
            var ds = OnGetUserDatabases(user);
            return GetAssociatedServerInstances(ds);
        }

        private Dictionary<string, ServerInstance> GetAssociatedServerInstances(Dictionary<string, SqlServerDataset> datasets)
        {
            var sis = new Dictionary<string, ServerInstance>(SchemaManager.Comparer);

            foreach (var key in datasets.Keys)
            {
                sis.Add(key, GetAssociatedServerInstance(datasets[key]));
            }

            return sis;
        }

        private ServerInstance GetAssociatedServerInstance(Jhu.Graywulf.Schema.SqlServer.SqlServerDataset dataset)
        {
            // This function is currently unused

            var hostname = dataset.HostName;
            var instancename = dataset.InstanceName;

            // Find the server instance based on Graywulf settings
            var mr = FederationContext.Federation.UserDatabaseVersion.ServerVersion.MachineRole;

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