using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Web.Security;

namespace Jhu.Graywulf.CasJobs
{
    [TestClass]
    public class UsersTest : CasJobsTestBase
    {
        [TestMethod]
        public void GetUserTest()
        {
            var ksuser = KeystoneClient.FindUsers("default", "test", false, false)[0];

            var user = Client.GetUser(ksuser.ID);
        }

        [TestMethod]
        public void CreateUserTest()
        {
            // TODO: fix this:
            // Unfortunately, the casjobs REST interface doesn't support deleting users
            // so the only way we can test is is to create a new user every time

            using (var context = ContextManager.Instance.CreateContext(ConnectionMode.AutoOpen, TransactionMode.AutoCommit))
            {
                var id = new KeystoneIdentityProvider(context.Domain);
                var name = "test_" + new Random().Next().ToString();

                var user = new Registry.User(context.Domain)
                {
                    Name = name,
                    Email = name + "@test.com",
                };

                id.CreateUser(user, "alma");
                id.ActivateUser(user);
                id.ResetPassword(user, "alma");

                //
                var token = KeystoneClient.Authenticate(user.Name, "alma");
                Keystone.KeystoneTokenCache.Instance.TryAdd(token);

                var udf = new CasJobsUserDatabaseFactory(new FederationContext(context, user));

                udf.GetUserDatabases(user);
            }
        }
    }
}
