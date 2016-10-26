using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jhu.Graywulf.Keystone
{
    [TestClass]
    public class TokenTest : KeystoneTestBase
    {
        [TestMethod]
        public void RenewTokenTest()
        {
            PurgeTestEntities();

            var domain = Client.GetDomain("default");
            var role = CreateTestRole();
            var project = CreateTestProject("user");
            var user = CreateTestUser("user");

            // Grant user the role
            Client.GrantRole(project, user, role);

            // Try once with password
            var token = Client.Authenticate(TestPrefix + "user", "alma", domain, project);

            // Now try to renew the token
            var token2 = Client.RenewToken(token);

            Assert.AreNotEqual(token.ID, token2.ID);
            Assert.AreEqual(token.Project.Domain.Name, token2.Project.Domain.Name);
            Assert.AreEqual(token.Project.Name, token2.Project.Name);
            Assert.IsTrue(token.ExpiresAt <= token2.ExpiresAt);

            PurgeTestEntities();
        }
    }
}
