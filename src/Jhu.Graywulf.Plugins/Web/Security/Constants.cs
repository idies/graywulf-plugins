using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.Graywulf.Web.Security
{
    class Constants
    {
        public const string AuthorityNameKeystone = "Keystone";

        public const string ProtocolNameOpenID = "OpenID";
        public const string ProtocolNameOAuth = "OAuth";
        public const string ProtocolNameOAuth2 = "OAuth2";
        public const string ProtocolNameKeystone = "Keystone";

        // TODO: delete
        public const string KeystoneDefaultUri = "http://localhost:5000/";
        public const string KeystoneDefaultDomain = "default";
        public const string KeystoneDefaultAuthTokenParameter = "keystoneToken";
        public const string KeystoneDefaultAuthTokenHeader = "X-Auth-Token";
        public const string KeystoneDefaultAuthTokenCookie = "X-Auth-Token";
        public const string KeystoneDefaultRole = "_member_";
        public const int KeystoneTokenRenewalInterval = 5;
    }
}
