using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Web;
using System.Web.Security;
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
    public class KeystoneAuthenticator : Authenticator
    {
        #region Static cache implementation

        private static readonly Cache<string, Token> tokenCache;

        static KeystoneAuthenticator()
        {
            tokenCache = new Cache<string, Token>(StringComparer.InvariantCultureIgnoreCase)
            {
                AutoExtendLifetime = false,
                CollectionInterval = new TimeSpan(0, 1, 0),     // one minute
                DefaultLifetime = new TimeSpan(0, 20, 0),       // twenty minutes
            };
        }

        #endregion
        #region Private member variables

        private KeystoneSettings settings;

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

        public KeystoneSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        #endregion
        #region Constructors and initializers

        public KeystoneAuthenticator()
        {
            InitializeMembers(new StreamingContext());
        }

        [OnDeserializing]
        private void InitializeMembers(StreamingContext context)
        {
            AuthorityName = Constants.AuthorityNameKeystone;

            this.settings = new KeystoneSettings();
        }

        #endregion

        public override void Initialize(Registry.Domain domain)
        {
            base.Initialize(domain);

            // Make sure settings holds same info as the authenticator
            settings.AuthorityName = this.AuthorityName;
            settings.AuthorityUri = this.AuthorityUri;
        }

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

            string tokenID = null;
            var foundInCookie = false;

            // Look for a token in a cookie
            var cookies = request.Cookies.GetCookies(request.Uri);
            if (cookies != null)
            {
                var cookie = cookies[settings.AuthTokenCookie];
                if (cookie != null)
                {
                    tokenID = cookie.Value;
                    foundInCookie = true;
                }
            }

            // Look for a token in the request headers
            if (tokenID == null)
            {
                tokenID = request.Headers[settings.AuthTokenHeader];
            }

            // Try to take header from the query string
            if (tokenID == null)
            {
                tokenID = request.QueryString[settings.AuthTokenParameter];
            }

            if (tokenID != null)
            {
                Token token;

                // Check if the resolved token is already in the cache
                if (!tokenCache.TryGetValue(tokenID, out token))
                {
                    // Need to validate token against Keystone
                    var ksclient = settings.CreateClient();

                    try
                    {
                        token = ksclient.GetToken(tokenID);
                    }
                    catch (System.Net.WebException)
                    {
                        // This is very likely a token not found exception (404)
                        // user cannot be authenticated this way
                        settings.UpdateAuthenticationResponse(response, null, IsMasterAuthority);

                        return;
                    }

                    token.User = ksclient.GetUser(token.User.ID);

                    tokenCache.TryAdd(token.ID, token);
                }

                // If the token is coming in a cookie and seems too old we can renew it here
                if (foundInCookie && (DateTime.Now.ToUniversalTime() - token.IssuedAt).TotalMinutes > settings.TokenRenewInterval)
                {
                    // Request new token here...
                    var ksclient = settings.CreateClient();
                    var newtoken = ksclient.RenewToken(token);
                    newtoken.User = ksclient.GetUser(newtoken.User.ID);

                    // Update cache
                    tokenCache.TryRemove(tokenID, out token);
                    tokenCache.TryAdd(newtoken.ID, newtoken, newtoken.ExpiresAt);

                    token = newtoken;

                    // TODO: Now the problem is if a user comes back with the old but still
                    // valid token
                }

                settings.UpdateAuthenticationResponse(response, token, IsMasterAuthority);
            }
        }
    }
}
