using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Web;
using System.Web.Security;
using System.Configuration;
using Jhu.Graywulf.Components;
using Jhu.Graywulf.Check;
using Jhu.Graywulf.AccessControl;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Keystone;

namespace Jhu.Graywulf.Web.Security
{

    /// <summary>
    /// Implements functions to authenticate an HTTP request based on
    /// a Keystone token in the header
    /// </summary>
    public class KeystoneAuthentication : Authentication, ICheckable
    {
        #region Static members
        public static KeystoneAuthenticationConfiguration Configuration
        {
            get
            {
                return (KeystoneAuthenticationConfiguration)ConfigurationManager.GetSection("jhu.graywulf/authentication/keystone");
            }
        }

        #endregion
        #region Properties

        public override string ProtocolName
        {
            get { return Constants.ProtocolNameKeystone; }
        }

        public override AuthenticatorProtocolType ProtocolType
        {
            get
            {
                return AuthenticatorProtocolType.WebRequest |
                       AuthenticatorProtocolType.RestRequest;
            }
        }

        #endregion
        #region Constructors and initializers

        public KeystoneAuthentication()
        {
            InitializeMembers();
        }

        private void InitializeMembers()
        {
            AuthorityName = KeystoneClient.Configuration.AuthorityName;
            AuthorityUri = KeystoneClient.Configuration.BaseUri;
            IsMasterAuthority = Configuration.IsMasterAuthority;
            DisplayName = Constants.ProtocolNameKeystone;
            IsEnabled = Configuration.IsEnabled;
        }

        #endregion

        public override void Authenticate(AuthenticationRequest request, AuthenticationResponse response)
        {
            // Keystone tokens (in the simplest case) do not carry any detailed
            // information about the identity of the user. For this reason,
            // every token needs to be validated by calling the Keystone service.
            // To avoid doing this in every single request, we need to cache tokens.

            // This function also supports token renewal with a configurable granularity.
            // We compare the token issue time with the current time and if it's older than a given
            // number of minutes we renew the token unconditionally. This effectively results
            // in sliding token expiration, though generates a lot of tickets.
            // Token renewal only happens when the token is conveyed in a cookie, meaning the
            // request is made by a browser or a smarter client.

            bool foundInCookie;
            var tokenID = GetTokenID(request, out foundInCookie);

            // // Only set cookie if request didn't already have it
            bool setParameter = true;
            bool setHeader = request.ProtocolType.HasFlag(AuthenticatorProtocolType.RestRequest);
            bool setCookie = !foundInCookie && request.ProtocolType.HasFlag(AuthenticatorProtocolType.WebRequest);

            if (tokenID != null)
            {
                Token token;

                // Check if the resolved token is already in the cache
                if (!KeystoneTokenCache.Instance.TryGetValueByTokenID(tokenID, out token))
                {
                    // If it's not in the cache we need to validate it against Keystone
                    token = ValidateTokenID(tokenID, request, response);
                }

                if (token != null && foundInCookie)
                {
                    Token newtoken;
                    if (RenewToken(token, out newtoken))
                    {
                        token = newtoken;
                        setCookie = true;
                    }
                }

                

                UpdateAuthenticationResponse(request, response, token, setParameter, setHeader, setCookie, IsMasterAuthority);
            }
        }

        public override void Reset(AuthenticationRequest request, AuthenticationResponse response)
        {
            DeleteAuthCookie(response);
        }

        internal static GraywulfPrincipal CreateAuthenticatedPrincipal(Keystone.User user, bool isMasterAuthority)
        {
            var config = Keystone.KeystoneClient.Configuration;

            // TODO: role logic might be added here

            var identity = new GraywulfIdentity()
            {
                Protocol = Constants.ProtocolNameKeystone,
                AuthorityName = config.AuthorityName,
                AuthorityUri = config.BaseUri.ToString(),
                Identifier = user.ID,
                IsAuthenticated = true,
                IsMasterAuthority = isMasterAuthority,
            };

            // Accept users without the following parameters set but
            // this is not a good practice in general to leave them null 
            // in Keystone
            var reguser = new Registry.User()
            {
                Name = user.Name,
                Comments = user.Description ?? String.Empty,
                Email = user.Email ?? String.Empty,
                DeploymentState = user.Enabled.Value ? Registry.DeploymentState.Deployed : Registry.DeploymentState.Undeployed,
            };

            var fqn = EntityFactory.CombineName(EntityType.User, ContextManager.Configuration.DomainName, user.Name);
            reguser.SetFullyQualifiedName(fqn);

            identity.User = reguser;

            return new GraywulfPrincipal(identity);
        }

        internal static void UpdateAuthenticationResponse(AuthenticationRequest request, AuthenticationResponse response, Token token, bool setParameter, bool setHeader, bool setCookie, bool isMasterAuthority)
        {
            var config = Configuration;

            // Forward identified user to response
            if (token != null && response.Principal == null)
            {
                var principal = CreateAuthenticatedPrincipal(token.User, isMasterAuthority);
                response.SetPrincipal(principal);
            }

            // Add keystone token to various response collections if necessary
            // This data may be used depending on the communication channel (i.e. browser, WCF)
            if (token != null)
            {
                if (setParameter && !String.IsNullOrWhiteSpace(config.AuthTokenParameter))
                {
                    response.QueryParameters.Add(config.AuthTokenParameter, token.ID);
                }

                if (setHeader &&
                    !String.IsNullOrWhiteSpace(config.AuthTokenHeader))
                {
                    response.Headers.Add(config.AuthTokenHeader, token.ID);
                }

                if (setCookie &&
                    !String.IsNullOrWhiteSpace(config.AuthTokenCookie))
                {
                    var cookie = new System.Web.HttpCookie(config.AuthTokenCookie, token.ID)
                    {
                        Expires = token.ExpiresAt,
                    };

                    response.Cookies.Add(cookie);
                }
            }
            else
            {
                DeleteAuthCookie(response);
            }
        }

        private static void DeleteAuthCookie(AuthenticationResponse response)
        {
            var config = Configuration;

            if (!String.IsNullOrWhiteSpace(config.AuthTokenCookie))
            {
                var cookie = new System.Web.HttpCookie(config.AuthTokenCookie, String.Empty)
                {
                    Expires = DateTime.Now.AddDays(-1)
                };

                response.Cookies.Add(cookie);
            }
        }

        /// <summary>
        /// Returns the token ID found in an HTTP or REST request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="foundInCookie"></param>
        /// <returns></returns>
        private string GetTokenID(AuthenticationRequest request, out bool foundInCookie)
        {
            var config = Configuration;
            string tokenID = null;

            // Look for a token in a cookie
            foundInCookie = false;

            var cookies = request.Cookies.GetCookies(request.Uri);
            if (cookies != null)
            {
                var cookie = cookies[config.AuthTokenCookie];
                if (cookie != null)
                {
                    tokenID = cookie.Value;
                    foundInCookie = true;
                }
            }

            // Look for a token in the request headers
            if (tokenID == null)
            {
                tokenID = request.Headers[config.AuthTokenHeader];
            }

            // Try to take header from the query string
            if (tokenID == null)
            {
                tokenID = request.QueryString[config.AuthTokenParameter];
            }

            return tokenID;
        }

        private Token ValidateTokenID(string tokenID, AuthenticationRequest request, AuthenticationResponse response)
        {
            Token token;
            var ksclient = new KeystoneClient();

            try
            {
                token = ksclient.GetToken(tokenID);
            }
            catch (KeystoneException)
            {
                // This exception happens when
                // -- user comes in with invalid token
                // -- the token is missing from the url or cookie
                // -- the token has expired but it's not in the cache to tell
                token = null;
            }

            // Look up user owning the token and update cache
            if (token != null)
            {
                token.User = ksclient.GetUser(token.User.ID);
                KeystoneTokenCache.Instance.TryAdd(token);
            }

            return token;
        }

        private bool RenewToken(Token token, out Token newtoken)
        {
            // If the token is coming in a cookie and seems too old we can renew it here
            if (Configuration.AutoTokenRenewal &&
                (token.ExpiresAt - DateTime.UtcNow).TotalMinutes < Configuration.TokenRenewalInterval)
            {
                // Request new token
                var ksclient = new KeystoneClient();
                newtoken = ksclient.RenewToken(token);
                newtoken.User = ksclient.GetUser(newtoken.User.ID);

                // Update cache
                KeystoneTokenCache.Instance.TryRemoveByTokenID(token.ID, out token);
                KeystoneTokenCache.Instance.TryAdd(newtoken);

                return true;

                // TODO: Now the problem is if a user comes back with the old but still
                // valid token
            }

            newtoken = null;
            return false;
        }

        public override IEnumerable<CheckRoutineBase> GetCheckRoutines()
        {
            yield return new KeystoneCheck();
        }
    }
}
