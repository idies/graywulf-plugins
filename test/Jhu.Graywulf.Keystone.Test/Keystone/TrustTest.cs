using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jhu.Graywulf.Keystone
{
    [TestClass]
    public class TrustTest : KeystoneTestBase
    {
        [TestMethod]
        public void ManipulateTrustTest()
        {
            // Purge test users
            PurgeTestEntities();

            // Create a project and a role
            var role = CreateTestRole();
            var project = CreateTestProject();

            // Create new users
            var trustor = CreateTestUser("trustor");
            var trustee = CreateTestUser("trustee");

            // Associate users with project via the role
            Client.GrantRole(project, trustor, role);
            Client.GrantRole(project, trustee, role);

            // Create token for user1
            var token1 = Client.Authenticate("default", TestPrefix + "trustor", "alma");

            // Make user1 trust user2
            var trust = new Trust()
            {
                ExpiresAt = DateTime.Now.AddDays(2).ToUniversalTime(),
                Impersonation = true,
                TrustorUserID = trustor.ID,
                TrusteeUserID = trustee.ID,
                RemainingUses = 5,
                ProjectID = project.ID,
                Roles = new[] { role }
            };

            Client.UserCredentials = new KeystoneCredentials()
            {
                TokenID = token1.ID,
            };
            trust = Client.Create(trust);

            // Try to impersonate user with trust
            var token2 = Client.Authenticate("default", TestPrefix + "trustee", "alma");

            var token3 = Client.Authenticate(token2, trust);

            // Try to get the trust by ID
            trust = Client.GetTrust(trust.ID);

            // List all trusts
            var trusts = Client.ListTrusts();
            Assert.IsTrue(trusts.Length > 0);

            // List trusts of the trustor
            trusts = Client.ListTrusts(trustor);
            Assert.AreEqual(1, trusts.Length);

            // List roles of the trust
            var roles = Client.ListRoles(trust);
            Assert.AreEqual(1, roles.Length);

            // Check if role is delegated by the trust
            Client.CheckRole(trust, role);

            // Get the role delegated by the trust
            role = Client.GetRole(trust, role);

            // Delete trust
            Client.Delete(trust);

            PurgeTestEntities();
        }
    }
}
