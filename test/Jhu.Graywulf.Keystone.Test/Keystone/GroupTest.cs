using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jhu.Graywulf.Keystone
{
    [TestClass]
    public class GroupTest : KeystoneTestBase
    {
        [TestMethod]
        public void ManipulateGroupTest()
        {
            PurgeTestEntities();

            // Create a new group and a user
            var group = CreateTestGroup();
            var user = CreateTestUser("user");

            // Try to modify group
            group.Name = TestPrefix + "group2";
            group = Client.Update(group);

            // Get group by id
            group = Client.GetGroup(group.ID);

            // List all groups
            var groups = Client.ListGroups();
            Assert.IsTrue(groups.Length > 0);

            // Add user to the group
            Client.AddToGroup(user, group);

            // Check group membership
            Client.CheckGroup(user, group);

            // List users of the group
            var users = Client.ListUsers(group);
            Assert.AreEqual(1, users.Length);
            
            // Remove from group
            Client.RemoveFromGroup(user, group);

            users = Client.ListUsers(group);
            Assert.AreEqual(0, users.Length);

            // Delete group and user
            Client.Delete(group);
            Client.Delete(user);

            PurgeTestEntities();
        }
    }
}
