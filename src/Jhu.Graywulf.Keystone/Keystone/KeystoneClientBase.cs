using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Jhu.Graywulf.Components;
using Jhu.Graywulf.SimpleRestClient;

namespace Jhu.Graywulf.Keystone
{
    /// <summary>
    /// Implements core function to be used with services interacting
    /// with Keystone or any other service supporting Keystone.
    /// </summary>
    public abstract class KeystoneClientBase : RestClient
    {
        #region Private member variables

        private KeystoneCredentials adminCredentials;
        private Token userToken;

        #endregion
        #region Properties

        public KeystoneCredentials AdminCredentials
        {
            get { return adminCredentials; }
            set { adminCredentials = value; }
        }

        /// <summary>
        /// Gets or sets the token used for user authentication
        /// </summary>
        public Token UserToken
        {
            get { return userToken; }
            set { userToken = value; }
        }

        #endregion
        #region Constructors and initializers

        protected KeystoneClientBase(Uri baseUri)
            : base(baseUri)
        {
        }

        private void InitializeMembers()
        {
            this.adminCredentials = null;
            this.userToken = null;
        }

        #endregion
        #region Authentication and token manipulation

        protected Token GetAdminToken()
        {
            return GetToken(adminCredentials);
        }

        protected Token GetUserToken()
        {
            throw new NotImplementedException();
        }

        private Token GetToken(KeystoneCredentials credentials)
        {
            Token token;

            // If tokenID is set, use that ID. This is typically the case when the admin token is used
            // instead of username and password to authenticate the administrator account.
            // Otherwise try to look up a token by username. If no valid token is found request a new one
            // using the password provided by caller code.

            if (credentials.TokenID != null)
            {
                return new Token()
                {
                    ID = credentials.TokenID
                };
            }
            else if (!KeystoneTokenCache.Instance.TryGetValueByUserName(credentials.ProjectName, credentials.UserName, out token))
            {
                // No valid token found, request a new one
                token = Authenticate(credentials.DomainID, credentials.ProjectName, credentials.UserName, credentials.Password);
                KeystoneTokenCache.Instance.TryAdd(token);
            }

            return token;
        }

        public Token Authenticate(string domain, string username, string password)
        {
            return Authenticate(domain, username, password, null, null);
        }

        public Token Authenticate(string domain, string project, string username, string password)
        {
            var p = new Keystone.Project()
            {
                Name = project
            };

            var d = new Keystone.Domain()
            {
                Name = domain
            };

            return Authenticate(domain, username, password, d, p);
        }

        // TODO: test
        public Token Authenticate(string username, string password, Domain scope)
        {
            return Authenticate(null, username, password, scope, null);
        }

        public Token Authenticate(string username, string password, Domain scopeDomain, Project scopeProject)
        {
            return Authenticate(null, username, password, scopeDomain, scopeProject);
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
                HttpMethod.Post, "/v3/auth/tokens", req, GetAdminToken());

            var authResponse = resMessage.Body;

            // Token value comes in the header
            authResponse.Token.ID = resMessage.Headers[Constants.KeystoneXSubjectTokenHeader].Value;

            return authResponse.Token;
        }

        public Token RenewToken(Token token)
        {
            var req = AuthRequest.CreateMessage(token);
            var resMessage = SendRequest<AuthRequest, AuthResponse>(
                HttpMethod.Post, "v3/auth/tokens", req);

            var authResponse = resMessage.Body;

            // Token value comes in the header
            authResponse.Token.ID = resMessage.Headers[Constants.KeystoneXSubjectTokenHeader].Value;

            return authResponse.Token;
        }

        public Token GetToken(string tokenID)
        {
            var headers = new RestHeaderCollection();
            headers.Add(new RestHeader(Constants.KeystoneXSubjectTokenHeader, tokenID));

            var resMessage = SendRequest<AuthResponse>(
                HttpMethod.Get, "/v3/auth/tokens", headers, GetAdminToken());

            var authResponse = resMessage.Body;

            // Token ID comes in the header
            authResponse.Token.ID = resMessage.Headers[Constants.KeystoneXSubjectTokenHeader].Value;

            return authResponse.Token;
        }

        public bool ValidateToken(Token token)
        {
            var headers = new RestHeaderCollection();
            headers.Add(new RestHeader(Constants.KeystoneXSubjectTokenHeader, token.ID));

            var resMessage = SendRequest(
                HttpMethod.Head, "/v3/auth/tokens", headers, GetAdminToken());

            return true;
        }

        public void RevokeToken(Token token)
        {
            var headers = new RestHeaderCollection();
            headers.Add(new RestHeader(Constants.KeystoneXSubjectTokenHeader, token.ID));

            var resMessage = SendRequest(
                HttpMethod.Delete, "/v3/auth/tokens", headers, GetAdminToken());
        }

        #endregion
        #region Specialized REST functions for Keystone

        protected RestHeaderCollection PreprocessHeaders(RestHeaderCollection headers, Token authToken)
        {
            if (headers == null)
            {
                headers = new RestHeaderCollection();
            }

            // Add authentication token
            headers.Set(new RestHeader(Constants.KeystoneXAuthTokenHeader, authToken.ID));

            return headers;
        }

        protected RestMessage<T> PreprocessHeaders<T>(RestMessage<T> message, Token authToken)
        {
            message.Headers = PreprocessHeaders(message.Headers, authToken);

            return message;
        }

        protected RestHeaderCollection SendRequest(HttpMethod method, string path, Token authToken)
        {
            return SendRequest(method, path, (RestHeaderCollection)null, authToken);
        }

        protected RestHeaderCollection SendRequest(HttpMethod method, string path, RestHeaderCollection headers, Token authToken)
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

        protected RestMessage<R> SendRequest<R>(HttpMethod method, string path, Token authToken)
        {
            return SendRequest<R>(method, path, (RestHeaderCollection)null, authToken);
        }

        protected RestMessage<R> SendRequest<R>(HttpMethod method, string path, RestHeaderCollection headers, Token authToken)
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

        protected RestHeaderCollection SendRequest<T>(HttpMethod method, string path, RestMessage<T> message, Token authToken)
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

        protected RestMessage<U> SendRequest<T, U>(HttpMethod method, string path, RestMessage<T> message, Token authToken)
        {
            try
            {
                return base.SendRequest<T, U>(method, path, PreprocessHeaders(message, authToken));
            }
            catch (RestException ex)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif

                throw CreateException(ex);
            }
        }

        protected abstract Exception CreateException(RestException ex);

        #endregion
    }
}
