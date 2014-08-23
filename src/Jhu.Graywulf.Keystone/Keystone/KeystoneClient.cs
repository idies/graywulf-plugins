using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using Jhu.Graywulf.SimpleRestClient;

namespace Jhu.Graywulf.Keystone
{
    /// <summary>
    /// Implements a client for the OpenStack Keystone v3 identity service
    /// REST interface.
    /// </summary>
    /// <remarks>
    /// https://github.com/openstack/identity-api/blob/master/v3/src/markdown/identity-api-v3.md
    ///
    /// Trusts can only be made by trustors, consequently the Keystone API
    /// expects a token identifying the trustor and not that of the admin.
    /// Always set the UserAuthToken property before calling this function.
    /// </remarks>
    public class KeystoneClient : RestClient
    {
        #region Private member variables

        private string adminAuthToken;
        private string userAuthToken;

        #endregion
        #region Properties

        public string AdminAuthToken
        {
            get { return adminAuthToken; }
            set { adminAuthToken = value; }
        }

        public string UserAuthToken
        {
            get { return userAuthToken; }
            set { userAuthToken = value; }
        }

        #endregion
        #region Constructors and initializers

        public KeystoneClient(Uri baseUri)
            : base(baseUri)
        {
        }

        #endregion
        #region Version

        public Version GetVersion()
        {
            var res = SendRequest<VersionResponse>(
                HttpMethod.Get, "/v3", adminAuthToken);

            return res.Body.Version;
        }

        #endregion
        #region Domain manipulation

        public Domain Create(Domain domain)
        {
            var req = DomainRequest.CreateMessage(domain);
            var res = SendRequest<DomainRequest, DomainResponse>(
                HttpMethod.Post, "/v3/domains", req, adminAuthToken);

            return res.Body.Domain;
        }

        public Domain Update(Domain domain)
        {
            var req = DomainRequest.CreateMessage(domain);
            var res = SendRequest<DomainRequest, DomainResponse>(
                HttpMethod.Patch, String.Format("/v3/domains/{0}", domain.ID), req, adminAuthToken);

            return res.Body.Domain;
        }

        public void Delete(Domain domain)
        {
            // Domain needs to be deleted first
            domain.Enabled = false;
            Update(domain);

            // Now it can be deleted
            SendRequest(HttpMethod.Delete, String.Format("/v3/domains/{0}", domain.ID), adminAuthToken);
        }

        public Domain GetDomain(string id)
        {
            var res = SendRequest<DomainResponse>(
                HttpMethod.Get, String.Format("/v3/domains/{0}", id), adminAuthToken);

            return res.Body.Domain;
        }

        public Domain[] ListDomains()
        {
            var res = SendRequest<DomainListResponse>(
                HttpMethod.Get, "/v3/domains", adminAuthToken);

            return res.Body.Domains;
        }

        #endregion
        #region Project manipulation

        public Project Create(Project project)
        {
            var req = ProjectRequest.CreateMessage(project);
            var res = SendRequest<ProjectRequest, ProjectResponse>(
                HttpMethod.Post, "/v3/projects", req, adminAuthToken);

            return res.Body.Project;
        }

        public Project Update(Project project)
        {
            var req = ProjectRequest.CreateMessage(project);
            var res = SendRequest<ProjectRequest, ProjectResponse>(
                HttpMethod.Patch, String.Format("/v3/projects/{0}", project.ID), req, adminAuthToken);

            return res.Body.Project;
        }

        public void Delete(Project project)
        {
            // Now it can be deleted
            SendRequest(HttpMethod.Delete, String.Format("/v3/projects/{0}", project.ID), adminAuthToken);
        }

        public Project GetProject(string id)
        {
            var res = SendRequest<ProjectResponse>(
                HttpMethod.Get, String.Format("/v3/projects/{0}", id), adminAuthToken);

            return res.Body.Project;
        }

        public Project[] ListProjects()
        {
            var res = SendRequest<ProjectListResponse>(
                HttpMethod.Get, "/v3/projects", adminAuthToken);

            return res.Body.Projects;
        }

        public Project[] ListProjects(string domainID)
        {
            var res = SendRequest<ProjectListResponse>(
                HttpMethod.Get, String.Format("/v3/domains/{0}/projects", domainID), adminAuthToken);

            return res.Body.Projects;
        }

        public Project[] FindProjects(string domainID, string name, bool enabledOnly, bool caseInsensitive)
        {
            var query = BuildSearchQueryString(domainID, name, enabledOnly, caseInsensitive);
            var res = SendRequest<ProjectListResponse>(
                HttpMethod.Get, "/v3/projects" + query, adminAuthToken);

            return res.Body.Projects;
        }

        #endregion
        #region Role manipulation

        public Role Create(Role role)
        {
            var req = RoleRequest.CreateMessage(role);
            var res = SendRequest<RoleRequest, RoleResponse>(
                HttpMethod.Post, "/v3/roles", req, adminAuthToken);

            return res.Body.Role;
        }

        public Role Update(Role project)
        {
            var req = RoleRequest.CreateMessage(project);
            var res = SendRequest<RoleRequest, RoleResponse>(
                HttpMethod.Patch, String.Format("/v3/roles/{0}", project.ID), req, adminAuthToken);

            return res.Body.Role;
        }

        public void Delete(Role role)
        {
            // Now it can be deleted
            SendRequest(HttpMethod.Delete, String.Format("/v3/roles/{0}", role.ID), adminAuthToken);
        }

        public Role GetRole(string id)
        {
            var res = SendRequest<RoleResponse>(
                HttpMethod.Get, String.Format("/v3/roles/{0}", id), adminAuthToken);

            return res.Body.Role;
        }

        public Role[] ListRoles()
        {
            var res = SendRequest<RoleListResponse>(
                HttpMethod.Get, "/v3/roles", adminAuthToken);

            return res.Body.Roles;
        }

        public Role[] ListRoles(string domainID)
        {
            var res = SendRequest<RoleListResponse>(
                HttpMethod.Get, String.Format("/v3/domains/{0}/roles", domainID), adminAuthToken);

            return res.Body.Roles;
        }

        #endregion
        #region Group manipulation

        public Group Create(Group group)
        {
            var req = GroupRequest.CreateMessage(group);
            var res = SendRequest<GroupRequest, GroupResponse>(
                HttpMethod.Post, "/v3/groups", req, adminAuthToken);

            return res.Body.Group;
        }

        public Group Update(Group group)
        {
            var req = GroupRequest.CreateMessage(group);
            var res = SendRequest<GroupRequest, GroupResponse>(
                HttpMethod.Patch, String.Format("/v3/groups/{0}", group.ID), req, adminAuthToken);

            return res.Body.Group;
        }

        public void Delete(Group group)
        {
            SendRequest(HttpMethod.Delete, String.Format("/v3/groups/{0}", group.ID), adminAuthToken);
        }

        public Group GetGroup(string id)
        {
            var res = SendRequest<GroupResponse>(
                HttpMethod.Get, String.Format("/v3/groups/{0}", id), adminAuthToken);

            return res.Body.Group;
        }

        public Group[] ListGroups()
        {
            var res = SendRequest<GroupListResponse>(
                HttpMethod.Get, "/v3/groups", adminAuthToken);

            return res.Body.Groups;
        }

        #endregion
        #region User manipulation

        public User Create(User user)
        {
            var req = UserRequest.CreateMessage(user);
            var res = SendRequest<UserRequest, UserResponse>(
                HttpMethod.Post, "/v3/users", req, adminAuthToken);

            return res.Body.User;
        }

        public User Update(User user)
        {
            var req = UserRequest.CreateMessage(user);
            var res = SendRequest<UserRequest, UserResponse>(
                HttpMethod.Patch, String.Format("/v3/users/{0}", user.ID), req, adminAuthToken);

            return res.Body.User;
        }

        public void Delete(User user)
        {
            SendRequest(HttpMethod.Delete, String.Format("/v3/users/{0}", user.ID), adminAuthToken);
        }

        public void ResetPassword(string userID, string newPassword)
        {
            var user = new User()
            {
                Password = newPassword
            };

            var req = UserRequest.CreateMessage(user);

            SendRequest<UserRequest>(
                HttpMethod.Patch, String.Format("/v3/users/{0}", userID), req, adminAuthToken);
        }

        public void ChangePassword(string userID, string oldPassword, string newPassword)
        {
            var user = new User()
            {
                OriginalPassword = oldPassword,
                Password = newPassword
            };

            var req = UserRequest.CreateMessage(user);

            SendRequest<UserRequest>(
                HttpMethod.Post, String.Format("/v3/users/{0}/password", userID), req, adminAuthToken);
        }

        public User GetUser(string id)
        {
            var res = SendRequest<UserResponse>(
                HttpMethod.Get, String.Format("/v3/users/{0}", id), adminAuthToken);

            return res.Body.User;
        }

        public User GetUser(Token token)
        {
            // Token might not contain user info, so authenticate with
            // it to get user id
            token = Authenticate(token);

            return GetUser(token.User.ID);
        }

        public User[] ListUsers()
        {
            var res = SendRequest<UserListResponse>(
                HttpMethod.Get, "/v3/users", adminAuthToken);

            return res.Body.Users;
        }

        public User[] ListUsers(Group group)
        {
            var res = SendRequest<UserListResponse>(
                HttpMethod.Get,
                String.Format("/v3/groups/{0}/users", group.ID),
                adminAuthToken);

            return res.Body.Users;
        }

        public User[] FindUsers(string domainID, string name, bool enabledOnly, bool caseInsensitive)
        {
            var query = BuildSearchQueryString(domainID, name, enabledOnly, caseInsensitive);
            var res = SendRequest<UserListResponse>(
                HttpMethod.Get, "/v3/users" + query, adminAuthToken);

            return res.Body.Users;
        }

        public void AddToGroup(User user, Group group)
        {
            SendRequest(
                HttpMethod.Put,
                String.Format("/v3/groups/{0}/users/{1} ", group.ID, user.ID),
                adminAuthToken);
        }

        public void RemoveFromGroup(User user, Group group)
        {
            SendRequest(
                HttpMethod.Delete,
                String.Format("/v3/groups/{0}/users/{1} ", group.ID, user.ID),
                adminAuthToken);
        }

        public void CheckGroup(User user, Group group)
        {
            SendRequest(
                HttpMethod.Head,
                String.Format("/v3/groups/{0}/users/{1} ", group.ID, user.ID),
                adminAuthToken);
        }

        public void GrantRole(Domain domain, User user, Role role)
        {
            SendRequest(
                HttpMethod.Put,
                String.Format("/v3/domains/{0}/users/{1}/roles/{2}", domain.ID, user.ID, role.ID),
                adminAuthToken);
        }

        public void RevokeRole(Domain domain, User user, Role role)
        {
            SendRequest(
                HttpMethod.Delete,
                String.Format("/v3/domains/{0}/users/{1}/roles/{2}", domain.ID, user.ID, role.ID),
                adminAuthToken);
        }

        public void CheckRole(Domain domain, User user, Role role)
        {
            SendRequest(
                HttpMethod.Head,
                String.Format("/v3/domains/{0}/users/{1}/roles/{2}", domain.ID, user.ID, role.ID),
                adminAuthToken);
        }

        public Role[] ListRoles(Domain domain, User user)
        {
            // TODO
            throw new NotImplementedException();
        }

        public void GrantRole(Project project, User user, Role role)
        {
            SendRequest(
                HttpMethod.Put,
                String.Format("/v3/projects/{0}/users/{1}/roles/{2}", project.ID, user.ID, role.ID),
                adminAuthToken);
        }

        public void RevokeRole(Project project, User user, Role role)
        {
            SendRequest(
                HttpMethod.Delete,
                String.Format("/v3/projects/{0}/users/{1}/roles/{2}", project.ID, user.ID, role.ID),
                adminAuthToken);
        }

        public void CheckRole(Project project, User user, Role role)
        {
            SendRequest(
                HttpMethod.Head,
                String.Format("/v3/projects/{0}/users/{1}/roles/{2}", project.ID, user.ID, role.ID),
                adminAuthToken);
        }

        public void ListRoles(Project project, User user)
        {
            // TODO
            throw new NotImplementedException();
        }

        #endregion
        #region Authentication and token manipulation

        public Token Authenticate(string domain, string username, string password)
        {
            return Authenticate(domain, username, password, null, null);
        }

        // TODO: test
        public Token Authenticate(string domain, string username, string password, Domain scope)
        {
            return Authenticate(domain, username, password, scope, null);
        }

        // TODO: test
        public Token Authenticate(string domain, string username, string password, Project scope)
        {
            return Authenticate(domain, username, password, null, scope);
        }

        private Token Authenticate(string domain, string username, string password, Domain scopeDomain, Project scopeProject)
        {
            var req = AuthRequest.CreateMessage(domain, username, password, scopeDomain, scopeProject);
            var resMessage = SendRequest<AuthRequest, AuthResponse>(
                HttpMethod.Post, "/v3/auth/tokens", req);

            var authResponse = resMessage.Body;

            // Token value comes in the header
            authResponse.Token.ID = resMessage.Headers[Constants.KeystoneXSubjectTokenHeader].Value;

            return authResponse.Token;
        }

        public Token Authenticate(Token token)
        {
            return Authenticate(token, null);
        }

        public Token Authenticate(Token token, Trust trust)
        {
            var req = AuthRequest.CreateMessage(token, trust);
            var resMessage = SendRequest<AuthRequest, AuthResponse>(
                HttpMethod.Post, "/v3/auth/tokens", req, adminAuthToken);

            var authResponse = resMessage.Body;

            // Token value comes in the header
            authResponse.Token.ID = resMessage.Headers[Constants.KeystoneXSubjectTokenHeader].Value;

            return authResponse.Token;
        }

        public bool ValidateToken(Token token)
        {
            var headers = new RestHeaderCollection();
            headers.Add(new RestHeader(Constants.KeystoneXSubjectTokenHeader, token.ID));

            var resMessage = SendRequest(
                HttpMethod.Head, "/v3/auth/tokens", headers, adminAuthToken);

            return true;
        }

        public void RevokeToken(Token token)
        {
            var headers = new RestHeaderCollection();
            headers.Add(new RestHeader(Constants.KeystoneXSubjectTokenHeader, token.ID));

            var resMessage = SendRequest(
                HttpMethod.Delete, "/v3/auth/tokens", headers, adminAuthToken);
        }

        #endregion
        #region Trusts

        /// <summary>
        /// Creates a new trust.
        /// </summary>
        /// <param name="trust"></param>
        /// <returns></returns>
        /// <remarks>
        /// This function requires the UserAuthToken property set to a valid
        /// token of the trustor.
        /// </remarks>
        public Trust Create(Trust trust)
        {
            var req = TrustRequest.CreateMessage(trust);
            var res = SendRequest<TrustRequest, TrustResponse>(
                HttpMethod.Post, "/v3/OS-TRUST/trusts", req, userAuthToken);

            return res.Body.Trust;
        }

        /// <summary>
        /// Creates a new trust.
        /// </summary>
        /// <param name="trustorToken"></param>
        /// <param name="trustee"></param>
        /// <param name="project"></param>
        /// <param name="role"></param>
        /// <param name="expiresAt"></param>
        /// <param name="uses"></param>
        /// <param name="impersonate"></param>
        /// <returns></returns>
        /// <remarks>
        /// This function requires the UserAuthToken property set to a valid
        /// token of the trustor.
        /// </remarks>
        public Trust Create(User trustee, Project project, Role role, DateTime expiresAt, int uses, bool impersonate)
        {
            // Get user based on trustor token and use
            // the token to create new trust
            var trustor = GetUser(userAuthToken);

            var trust = new Trust()
            {
                ExpiresAt = expiresAt.ToUniversalTime(),
                Impersonation = impersonate,
                TrustorUserID = trustor.ID,
                TrusteeUserID = trustee.ID,
                RemainingUses = uses,
                ProjectID = project.ID,
                Roles = new[] { role }
            };

            return Create(trust);
        }

        /// <remarks>
        /// This function requires the UserAuthToken property set to a valid
        /// token of the trustor.
        /// </remarks>
        public void Delete(Trust trust)
        {
            SendRequest(
                HttpMethod.Delete,
                String.Format("/v3/OS-TRUST/trusts/{0}", trust.ID),
                userAuthToken);
        }

        /// <remarks>
        /// This function requires the UserAuthToken property set to a valid
        /// token of the trustor.
        /// </remarks>
        public Trust GetTrust(string id)
        {
            var res = SendRequest<TrustResponse>(
                HttpMethod.Get,
                String.Format("/v3/OS-TRUST/trusts/{0}", id),
                userAuthToken);

            return res.Body.Trust;
        }

        public Trust[] ListTrusts()
        {
            var res = SendRequest<TrustListResponse>(
                HttpMethod.Get, "/v3/OS-TRUST/trusts", adminAuthToken);

            return res.Body.Trusts;
        }

        /// <remarks>
        /// This function requires the UserAuthToken property set to a valid
        /// token of the trustor.
        /// </remarks>
        public Trust[] ListTrusts(User trustor)
        {
            var res = SendRequest<TrustListResponse>(
                HttpMethod.Get,
                String.Format("/v3/OS-TRUST/trusts?trustor_user_id={0}", trustor.ID),
                userAuthToken);

            return res.Body.Trusts;
        }

        /// <remarks>
        /// This function requires the UserAuthToken property set to a valid
        /// token of the trustor.
        /// </remarks>
        public Role[] ListRoles(Trust trust)
        {
            var res = SendRequest<RoleListResponse>(
                HttpMethod.Get,
                String.Format("/v3/OS-TRUST/trusts/{0}/roles", trust.ID),
                userAuthToken);

            return res.Body.Roles;
        }

        /// <summary>
        /// Returns a list of roles within a domain matched by a pattern.
        /// </summary>
        /// <param name="domainID"></param>
        /// <param name="name"></param>
        /// <param name="enabledOnly"></param>
        /// <param name="caseInsensitive"></param>
        /// <returns></returns>
        public Role[] FindRoles(string domainID, string name, bool enabledOnly, bool caseInsensitive)
        {
            var query = BuildSearchQueryString(domainID, name, enabledOnly, caseInsensitive);
            var res = SendRequest<RoleListResponse>(
                HttpMethod.Get, "/v3/roles" + query, adminAuthToken);

            return res.Body.Roles;
        }

        /// <summary>
        /// Checks if the given role is delegated by the trust.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <remarks>
        /// This function requires the UserAuthToken property set to a valid
        /// token of the trustor.
        /// </remarks>
        public void CheckRole(Trust trust, Role role)
        {
            SendRequest(
                HttpMethod.Head,
                String.Format("/v3/OS-TRUST/trusts/{0}/roles/{1}", trust.ID, role.ID),
                userAuthToken);
        }

        /// <summary>
        /// Returns the role delegated by the trust
        /// </summary>
        /// <param name="trust"></param>
        /// <returns></returns>
        /// <remarks>
        /// This function requires the UserAuthToken property set to a valid
        /// token of the trustor.
        /// </remarks>
        public Role GetRole(Trust trust, Role role)
        {
            var res = SendRequest<RoleResponse>(
                HttpMethod.Get,
                String.Format("/v3/OS-TRUST/trusts/{0}/roles/{1}", trust.ID, role.ID),
                userAuthToken);

            return res.Body.Role;
        }

        #endregion
        #region Specialized request functions

        private string BuildSearchQueryString(string domainID, string name, bool enabledOnly, bool caseInsensitive)
        {
            // NOTE: inexact filtering works in Keystone v3.2 only!

            // Build query string
            var query = "";

            if (domainID != null)
            {
                query += "&domain_id=" + domainID;
            }

            if (name != null)
            {
                name = name.Trim();

                query += "&name";

                // Check if wildcard is used
                string method = null;
                var startstar = name.StartsWith("*");
                var endstar = name.EndsWith("*");

                if (startstar && endstar)
                {
                    method = "contains";
                }
                else if (startstar)
                {
                    method = "endswith";
                }
                else if (endstar)
                {
                    method = "startswith";
                }

                // Check if case insensitive
                string caseis = caseInsensitive ? "i" : "";

                if (method != null)
                {
                    query += "_" + caseis + method;
                }

                query += "=" + UrlEncode(name.Trim('*'));
            }

            if (enabledOnly)
            {
                query += "&enabled";
            }

            if (query != "")
            {
                query = "?" + query.Substring(1);
            }

            return query;
        }

        private RestHeaderCollection PreprocessHeaders(RestHeaderCollection headers, string authToken)
        {
            if (headers == null)
            {
                headers = new RestHeaderCollection();
            }

            // Add authentication token
            headers.Set(new RestHeader(Constants.KeystoneXAuthTokenHeader, authToken));

            return headers;
        }

        private RestMessage<T> PreprocessHeaders<T>(RestMessage<T> message, string authToken)
        {
            message.Headers = PreprocessHeaders(message.Headers, authToken);

            return message;
        }

        private RestHeaderCollection SendRequest(HttpMethod method, string path, string authToken)
        {
            return SendRequest(method, path, (RestHeaderCollection)null, authToken);
        }

        protected RestHeaderCollection SendRequest(HttpMethod method, string path, RestHeaderCollection headers, string authToken)
        {
            try
            {
                return base.SendRequest(method, path, PreprocessHeaders(headers, authToken));
            }
            catch (RestException ex)
            {
                throw CreateException(ex);
            }
        }

        private RestMessage<R> SendRequest<R>(HttpMethod method, string path, string authToken)
        {
            return SendRequest<R>(method, path, (RestHeaderCollection)null, authToken);
        }

        protected RestMessage<R> SendRequest<R>(HttpMethod method, string path, RestHeaderCollection headers, string authToken)
        {
            try
            {
                return base.SendRequest<R>(method, path, PreprocessHeaders(headers, authToken));
            }
            catch (RestException ex)
            {
                throw CreateException(ex);
            }
        }

        protected RestHeaderCollection SendRequest<T>(HttpMethod method, string path, RestMessage<T> message, string authToken)
        {
            try
            {
                return base.SendRequest<T>(method, path, PreprocessHeaders(message, authToken));
            }
            catch (RestException ex)
            {
                throw CreateException(ex);
            }
        }

        protected RestMessage<U> SendRequest<T, U>(HttpMethod method, string path, RestMessage<T> message, string authToken)
        {
            try
            {
                return base.SendRequest<T, U>(method, path, PreprocessHeaders(message, authToken));
            }
            catch (RestException ex)
            {
                throw CreateException(ex);
            }
        }

        /// <summary>
        /// Creates a Keystone exception from a generic REST exception.
        /// </summary>
        /// <remarks>
        /// Interprets the response body as a ErrorResponse and reads
        /// properties.
        /// </remarks>
        /// <param name="ex"></param>
        /// <returns></returns>
        private KeystoneException CreateException(RestException ex)
        {
            KeystoneException kex = null;
            var error = DeserializeJson<ErrorResponse>(ex.Body);

            if (error != null)
            {
                kex = new KeystoneException(error.Error.Message, ex)
                {
                    Title = error.Error.Title,
                    StatusCode = (HttpStatusCode)error.Error.Code
                };
            }
            else if (ex.InnerException is WebException)
            {
                kex = new KeystoneException(ex.Message, ex)
                {
                    StatusCode = ((HttpWebResponse)((WebException)ex.InnerException).Response).StatusCode
                };
            }
            else
            {
                kex = new KeystoneException(ex.Message, ex);
            }

            return kex;
        }

        #endregion
    }
}
