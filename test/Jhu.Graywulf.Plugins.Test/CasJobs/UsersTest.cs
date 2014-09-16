using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jhu.Graywulf.CasJobs
{
    [TestClass]
    public class UsersTest : CasJobsTestBase
    {
        [TestMethod]
        public void GetUserTest()
        {
            var ksuser = KeystoneClient.FindUsers("default", "test1", false, false)[0];

            Client.AdminCredentials = new Keystone.KeystoneCredentials()
            {
                DomainID = "default",
                ProjectName = "admin",
                UserName = "admin",
                Password = "almafa",
            };

            var user = Client.GetUser(ksuser.ID);
        }

        [TestMethod]
        public void CreateUserTest()
        {
            Client.AdminCredentials = new Keystone.KeystoneCredentials()
            {
                DomainID = "default",
                ProjectName = "admin",
                UserName = "admin",
                Password = "almafa",
            };

            var ksuser = KeystoneClient.FindUsers("default", "test1", false, false)[0];

            var user = new User()
            {
                UserId = ksuser.Name,
                Password = "alma",
                Email = ksuser.Email,
                FullName = !String.IsNullOrWhiteSpace(ksuser.Description) ? ksuser.Description : ksuser.Name,
            };

            Client.Create(ksuser.ID, user);

            // Now submit a dummy job to force mydb creation

            Client.UserToken = KeystoneClient.Authenticate("default", "test1", "test1", "alma");

            var query = new Query()
            {
                QueryText = "SELECT 1 AS a",
                TaskName = "Dummy task to force MyDB creation."
            };

            Client.Submit("mydb", query);
        }
    }
}
