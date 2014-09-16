using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.Graywulf.Keystone
{
    public class KeystoneTestBase
    {
        protected const string TestPrefix = "__test__";

        private KeystoneClient client;

        protected KeystoneClient Client
        {
            get
            {
                if (client == null)
                {
                    client = new KeystoneClient();
                }

                return client;
            }
        }

        protected void PurgeTestEntities()
        {
            // This is a dangerour operation, never run on
            // the live server

            var domains = Client.ListDomains();
            for (int i = 0; i < domains.Length; i++)
            {
                if (domains[i].Name.StartsWith(TestPrefix))
                {
                    Client.Delete(domains[i]);
                }
            }

            var projects = Client.ListProjects();
            for (int i = 0; i < projects.Length; i++)
            {
                if (projects[i].Name.StartsWith(TestPrefix))
                {
                    Client.Delete(projects[i]);
                }
            }

            var roles = Client.ListRoles();
            for (int i = 0; i < roles.Length; i++)
            {
                if (roles[i].Name.StartsWith(TestPrefix))
                {
                    Client.Delete(roles[i]);
                }
            }

            var groups = Client.ListGroups();
            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i].Name.StartsWith(TestPrefix))
                {
                    Client.Delete(groups[i]);
                }
            }

            var users = Client.FindUsers(null, TestPrefix + "*", false, false);
            for (int i = 0; i < users.Length; i++)
            {
                if (users[i].Name.StartsWith(TestPrefix))
                {
                    Client.Delete(users[i]);
                }
            }
        }

        protected Domain CreateTestDomain()
        {
            var domain = new Domain()
            {
                Name = TestPrefix + "domain",
                Description = "test domain"
            };

            return Client.Create(domain);
        }

        protected Project CreateTestProject()
        {
            var project = new Project()
            {
                Name = TestPrefix + "project",
                Description = "test project",
            };

            return Client.Create(project);
        }

        protected Role CreateTestRole()
        {
            var role = new Role()
            {
                Name = TestPrefix + "role",
                Description = "test role",
            };

            return Client.Create(role);
        }

        protected Group CreateTestGroup()
        {
            var group = new Group()
            {
                Name = TestPrefix + "group",
                Description = "test group",
            };

            return Client.Create(group);
        }

        protected User CreateTestUser(string name)
        {

            var user = new User()
            {
                Name = TestPrefix + name,
                Description = "test user",
                Password = "alma",
            };

            return Client.Create(user);
        }
    }
}
