using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Jhu.Graywulf.Check;
using Jhu.Graywulf.Registry;

namespace Jhu.Graywulf.Web.Security
{
    /// <summary>
    /// Implements function to act as identity provider based on
    /// user data stored in a keystone instance. It created shadow
    /// users in the Graywulf registry
    /// </summary>
    public class KeystoneIdentityProvider : GraywulfIdentityProvider, ICheckable
    {
        #region Properties

        /// <summary>
        /// Gets a configures Keystone client class.
        /// </summary>
        internal Jhu.Graywulf.Keystone.KeystoneClient KeystoneClient
        {
            get
            {
                return new Keystone.KeystoneClient();
            }
        }

        #endregion
        #region Constructors and initializers

        public KeystoneIdentityProvider(Domain domain)
            : base(domain)
        {
            InitializeMembers();
        }

        private void InitializeMembers()
        {
        }

        #endregion

        /// <summary>
        /// Converts a Graywulf user to a matching Keystone user
        /// </summary>
        /// <param name="graywulfUser"></param>
        /// <returns></returns>
        private Jhu.Graywulf.Keystone.User ConvertUser(User graywulfUser)
        {
            var config = Keystone.KeystoneClient.Configuration;

            var keystoneUser = new Keystone.User()
            {
                Name = graywulfUser.Name,
                DomainID = config.Domain,
                Description = graywulfUser.Comments,
                Email = graywulfUser.Email,
                Enabled = graywulfUser.DeploymentState == DeploymentState.Deployed,
            };

            if (graywulfUser.IsExisting)
            {
                if (!graywulfUser.Context.IsValid)
                {
                    graywulfUser.Context = this.Context;
                }

                graywulfUser.LoadUserIdentities(false);

                var idname = GetIdentityName(graywulfUser);
                if (graywulfUser.UserIdentities.ContainsKey(idname))
                {
                    var uid = graywulfUser.UserIdentities[idname];
                    var idx = uid.Identifier.LastIndexOf("/");
                    keystoneUser.ID = uid.Identifier.Substring(idx + 1);
                }
                else
                {
                    // TODO: we might add logic here, to create user in keystone
                    throw new IdentityProviderException("No matching identity found."); // TODO
                }
            }

            return keystoneUser;
        }

        /// <summary>
        /// Returns a generated name for the UserIdentity entry
        /// in the Graywulf registry.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string GetIdentityName(User user)
        {
            var config = Keystone.KeystoneClient.Configuration;

            return String.Format("{0}_{1}", config.AuthorityName, user.Name);
        }

        #region User account manipulation

        private Keystone.User GetKeystoneUser(string username)
        {
            var config = Keystone.KeystoneClient.Configuration;

            // Try to get user from Keystone
            var users = KeystoneClient.FindUsers(config.Domain, username, false, false);

            if (users == null)
            {
                return null;
            }
            else if (users.Length != 1)
            {
                // This shouldn't happen but let's throw an exception to
                // make sure nothing goes wrong
                throw new InvalidOperationException();
            }
            else
            {
                return users[0];
            }
        }

        /// <summary>
        /// Returns a user looked up by username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        /// <remarks>
        /// If a user exists in the Keystone registry but not in Graywulf,
        /// a Graywulf user is returned initialized to values from Keystone
        /// but not saved automatically to the Graywulf registry.
        /// </remarks>
        public override User GetUserByUserName(string username)
        {
            var user = GetKeystoneUser(username);

            if (user == null)
            {
                return null;
            }
            else
            {
                // Try to get user from the registry. If not found, return null       
                return base.GetUserByUserName(username);
            }
        }

        protected override void OnCreateUser(User user, string password)
        {
            var config = Keystone.KeystoneClient.Configuration;

            // Create and associated project (tenant)
            var keystoneProject = new Keystone.Project
            {
                Name = user.Name,
                DomainID = config.Domain,
            };
            keystoneProject = KeystoneClient.Create(keystoneProject);

            // Create user in keystone
            var keystoneUser = ConvertUser(user);
            keystoneUser.DefaultProjectID = keystoneProject.ID;
            keystoneUser = KeystoneClient.Create(keystoneUser);
            KeystoneClient.ResetPassword(keystoneUser.ID, password);

            // Create user locally in Graywulf registry
            base.OnCreateUser(user, password);

            // Add newly created user to the default role
            var roles = KeystoneClient.FindRoles(null, KeystoneAuthentication.Configuration.DefaultRole, true, false);
            if (roles == null || roles.Length == 0)
            {
                throw new Exception("No matching role found");      // TODO: ***

                // TODO: we might need to automatically create roles in graywulf
                // once a new role in Keystone found
            }
            var role = roles[0];
            KeystoneClient.GrantRole(keystoneProject, keystoneUser, role);

            // Add identity to local principal
            var principal = KeystoneAuthentication.CreateAuthenticatedPrincipal(keystoneUser, true);
            AddUserIdentity(user, principal.Identity);
        }

        public override void ModifyUser(User user)
        {
            // Modify local shadow
            base.ModifyUser(user);

            // Update user in keystone
            KeystoneClient.Update(ConvertUser(user));
        }

        public override void DeleteUser(User user)
        {
            // Delete local shadow
            base.DeleteUser(user);

            // Delete user from keystone
            KeystoneClient.Delete(ConvertUser(user));
        }

        public override bool IsNameExisting(string username)
        {
            var config = Keystone.KeystoneClient.Configuration;

            // Try to get user from Keystone
            var users = KeystoneClient.FindUsers(config.Domain, username, false, false);

            return users != null && users.Length > 0;
        }

        #endregion
        #region User activation

        public override bool IsUserActive(User user)
        {
            var keystoneUser = ConvertUser(user);
            keystoneUser = KeystoneClient.GetUser(keystoneUser.ID);

            return keystoneUser.Enabled.Value;
        }

        public override void ActivateUser(User user)
        {
            base.ActivateUser(user);

            var keystoneUser = ConvertUser(user);
            keystoneUser.Enabled = true;
            KeystoneClient.Update(keystoneUser);
        }

        public override void DeactivateUser(User user)
        {
            base.DeactivateUser(user);

            var keystoneUser = ConvertUser(user);
            keystoneUser.Enabled = false;
            KeystoneClient.Update(keystoneUser);
        }

        #endregion
        #region Password functions

        public override AuthenticationResponse VerifyPassword(AuthenticationRequest request)
        {
            var config = Keystone.KeystoneClient.Configuration;

            // User needs to be authenticated in the scope of a project (tenant).
            // Since a tenant with the same name is generated for each user in keystone, we
            // use the username as project name.
            var project = new Keystone.Project()
            {
                Name = request.Username
            };

            var domain = new Keystone.Domain()
            {
                Name = config.Domain
            };

            // Verify user password in Keystone, we don't use
            // Graywulf password in this case
            var token = KeystoneClient.Authenticate(request.Username, request.Password, domain, project);

            // Find user details in keystone
            token.User = GetKeystoneUser(request.Username);

            // Add token to the cache to be used by subsequent requests.
            Keystone.KeystoneTokenCache.Instance.TryAdd(token);

            // Create a response, this sets necessary response headers
            var response = new AuthenticationResponse(request);
            KeystoneAuthentication.UpdateAuthenticationResponse(response, token, true);

            // Load user from the graywulf registry. This call will create the user
            // if necessary because authority is set to master
            LoadOrCreateUser(response.Principal);

            return response;
        }

        public override void ChangePassword(User user, string oldPassword, string newPassword)
        {
            var keystoneUser = ConvertUser(user);

            KeystoneClient.ChangePassword(keystoneUser.ID, oldPassword, newPassword);
        }

        public override void ResetPassword(User user, string newPassword)
        {
            var keystoneUser = ConvertUser(user);

            KeystoneClient.ResetPassword(keystoneUser.ID, newPassword);
        }

        #endregion

        /// <summary>
        /// Returns a valid keystone token from the token cache associated with the user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Keystone.Token GetCachedToken(Registry.User user)
        {
            var ksuser = ConvertUser(user);

            // Try to find a valid token in the cache
            Keystone.Token token;
            if (!Keystone.KeystoneTokenCache.Instance.TryGetValueByUserName(ksuser.Name, ksuser.Name, out token))
            {
                throw new UnauthorizedAccessException("Keystone token required.");
            }

            return token;
        }

        private Keystone.Trust CreateTrust(Registry.User user, TimeSpan expiresAfter)
        {
            var token = GetCachedToken(user);

            var trust = new Keystone.Trust()
            {
                ExpiresAt = DateTime.Now.Add(expiresAfter),
                Impersonation = true,
                TrustorUserID = token.User.ID,
                TrusteeUserID = token.User.ID,  // ???
                RemainingUses = 5,
                ProjectID = token.Project.ID,
                Roles = null  //  new[] { token } // ?????
            };

            var ksclient = new Keystone.KeystoneClient();
            trust = ksclient.Create(trust);

            return trust;
        }

        #region Check routines

        public override IEnumerable<CheckRoutineBase> GetCheckRoutines()
        {
            yield return new KeystoneCheck();
        }

        #endregion
    }
}
