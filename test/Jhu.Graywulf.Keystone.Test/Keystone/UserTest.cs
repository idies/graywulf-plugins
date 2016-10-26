using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jhu.Graywulf.Keystone
{
    /// <summary>
    /// Implements test for the keystone client.
    /// </summary>
    /// <remarks>
    /// The test requires a running Keystone installation on an URL
    /// accessible from the test machine and an admin token.
    /// </remarks>
    [TestClass]
    public class UserTest : KeystoneTestBase
    {
        [TestMethod]
        public void GetVersionTest()
        {
            var version = Client.GetVersion();

            Assert.AreEqual("v3.", version.ID.Substring(0, 3));
            Assert.AreEqual("stable", version.Status);
        }

        [TestMethod]
        public void ManipulateUserTest()
        {
            // Purge test users
            PurgeTestEntities();

            // Create a new test user
            var nuser = CreateTestUser("user");
            Assert.IsTrue(nuser.Enabled.Value);

            // Retreive test user by id
            nuser = Client.GetUser(nuser.ID);

            // Rename test user
            nuser.Name = TestPrefix + "user2";
            nuser = Client.Update(nuser);

            // Change password
            Client.ChangePassword(nuser.ID, "alma", "korte");

            Client.Delete(nuser);
        }

        [TestMethod]
        public void FindUsersTest()
        {
            PurgeTestEntities();

            CreateTestUser("user1");
            CreateTestUser("user2");

            // Exact match should not return anything
            var users = Client.FindUsers(null, TestPrefix + "user", false, false);
            Assert.AreEqual(0, users.Length);

            // Wildcard match returns the test user
            //users = Client.FindUsers(null, TestPrefix, false, false);
            //Assert.AreEqual(2, users.Length);

            // Exact match returns the single test user
            users = Client.FindUsers(null, TestPrefix + "user2", false, false);
            Assert.AreEqual(1, users.Length);

            // List all users
            users = Client.ListUsers();
            Assert.IsTrue(users.Length > 0);

            PurgeTestEntities();
        }

        [TestMethod]
        public void AuthenticationTest()
        {
            PurgeTestEntities();

            var project = CreateTestProject("user");
            var role = CreateTestRole();
            var user = CreateTestUser("user");

            Client.GrantRole(project, user, role);

            // Try once with password
            var token = Client.Authenticate("default", TestPrefix + "user", "alma");

            // Try now with token
            token = Client.Authenticate(token);

            // Get user from token
            user = Client.GetUser(token);
            Assert.AreEqual(TestPrefix + "user", user.Name);

            // Check token validity
            Client.ValidateToken(token);

            // Revoke token
            Client.RevokeToken(token);

            // Validation token now should throw an exception
            try
            {
                Client.ValidateToken(token);
                Assert.Fail();
            }
            catch (KeystoneException)
            {
            }

            PurgeTestEntities();
        }

        [TestMethod]
        public void RolesOnDomainTest()
        {
            PurgeTestEntities();

            var domain = Client.GetDomain("default");
            var role = CreateTestRole();
            var user = CreateTestUser("user");

            Client.GrantRole(domain, user, role);
            Client.CheckRole(domain, user, role);

            Client.RevokeRole(domain, user, role);
            try
            {
                Client.CheckRole(domain, user, role);
                Assert.Fail();
            }
            catch (KeystoneException)
            {
            }

            PurgeTestEntities();
        }

        [TestMethod]
        public void RolesOnProjectTest()
        {
            PurgeTestEntities();

            var project = CreateTestProject("user");
            var role = CreateTestRole();
            var user = CreateTestUser("user");

            Client.GrantRole(project, user, role);
            Client.CheckRole(project, user, role);

            Client.RevokeRole(project, user, role);
            try
            {
                Client.CheckRole(project, user, role);
                Assert.Fail();
            }
            catch (KeystoneException)
            {
            }

            PurgeTestEntities();
        }
    }
}
