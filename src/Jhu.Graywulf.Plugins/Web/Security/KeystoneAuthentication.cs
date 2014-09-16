using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Web;
using System.Web.Security;
using System.Configuration;
using System.Runtime.Serialization;
using Jhu.Graywulf.Components;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Keystone;

namespace Jhu.Graywulf.Web.Security
{

    /// <summary>
    /// Implements functions to authenticate an HTTP request based on
    /// a Keystone token in the header
    /// </summary>
    public class KeystoneAuthentication : Authenticator
    {
        #region Static members
        public static KeystoneAuthenticationConfiguration Configuration
        {
            get
            {
                return (KeystoneAuthenticationConfiguration)ConfigurationManager.GetSection("Jhu.Graywulf/Keystone/Authentication");
            }
        }

        #endregion

        // TODO: delete this and use global cache
        #region Static cache implementation

        private static readonly Cache<string, Token> tokenCache;

        static KeystoneAuthentication()
        {
            tokenCache = new Cache<string, Token>(StringComparer.InvariantCultureIgnoreCase)
            {
                AutoExtendLifetime = false,
                CollectionInterval = new TimeSpan(0, 1, 0),     // one minute
                DefaultLifetime = new TimeSpan(0, 20, 0),       // twenty minutes
            };
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
            InitializeMembers(new StreamingContext());
        }

        [OnDeserializing]
        private void InitializeMembers(StreamingContext context)
        {
            AuthorityName = KeystoneClient.Configuration.AuthorityName;
        }

        #endregion

        /* TODO: delete
        public override void Initialize(Registry.Domain domain)
        {
            base.Initialize(domain);

            // Make sure settings holds same info as the authenticator
            settings.AuthorityName = this.AuthorityName;
            settings.AuthorityUri = this.AuthorityUri;
        }
        */

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

            var config = Configuration;

            string tokenID = null;
            var foundInCookie = false;

            // Look for a token in a cookie
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

            if (tokenID != null)
            {
                Token token;

                // Check if the resolved token is already in the cache
                if (!tokenCache.TryGetValue(tokenID, out token))
                {
                    // Need to validate token against Keystone
                    var ksclient = new KeystoneClient();

                    try
                    {
                        token = ksclient.GetToken(tokenID);
                    }
                    catch (System.Net.WebException)
                    {
                        // This is very likely a token not found exception (404)
                        // user cannot be authenticated this way
                        UpdateAuthenticationResponse(response, null, IsMasterAuthority);

                        return;
                    }

                    token.User = ksclient.GetUser(token.User.ID);

                    tokenCache.TryAdd(token.ID, token);
                }

                // If the token is coming in a cookie and seems too old we can renew it here
                if (foundInCookie && (DateTime.Now.ToUniversalTime() - token.IssuedAt).TotalMinutes > config.TokenRenewalInterval)
                {
                    // Request new token here...
                    var ksclient = new KeystoneClient();
                    var newtoken = ksclient.RenewToken(token);
                    newtoken.User = ksclient.GetUser(newtoken.User.ID);

                    // Update cache
                    tokenCache.TryRemove(tokenID, out token);
                    tokenCache.TryAdd(newtoken.ID, newtoken, newtoken.ExpiresAt);

                    token = newtoken;

                    // TODO: Now the problem is if a user comes back with the old but still
                    // valid token
                }

                UpdateAuthenticationResponse(response, token, IsMasterAuthority);
            }
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
            identity.User = new Registry.User()
            {
                Name = user.Name,
                Comments = user.Description ?? String.Empty,
                Email = user.Email ?? String.Empty,
                DeploymentState = user.Enabled.Value ? Registry.DeploymentState.Deployed : Registry.DeploymentState.Undeployed,
            };

            return new GraywulfPrincipal(identity);
        }

        internal static void UpdateAuthenticationResponse(AuthenticationResponse response, Token token, bool isMasterAuthority)
        {
            var config = Configuration;

            // Forward identified user to response
            if (token != null && response.Principal == null)
            {
                var principal = CreateAuthenticatedPrincipal(token.User, isMasterAuthority);
                response.SetPrincipal(principal);
            }

            // Add keystone token to various response collections in necessary
            // This data may be used depending on the communication channel (i.e. browser, WCF)
            if (token != null)
            {
                if (!String.IsNullOrWhiteSpace(config.AuthTokenParameter))
                {
                    response.QueryParameters.Add(config.AuthTokenParameter, token.ID);
                }

                if (!String.IsNullOrWhiteSpace(config.AuthTokenHeader))
                {
                    response.Headers.Add(config.AuthTokenHeader, token.ID);
                }

                if (!String.IsNullOrWhiteSpace(config.AuthTokenCookie))
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
                // Delete cookie
                if (!String.IsNullOrWhiteSpace(config.AuthTokenCookie))
                {
                    var cookie = new System.Web.HttpCookie(config.AuthTokenCookie, String.Empty)
                    {
                        Expires = DateTime.Now.AddDays(-1)
                    };
                    response.Cookies.Add(cookie);
                }
            }
        }
    }
}
