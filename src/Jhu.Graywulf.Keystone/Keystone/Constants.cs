using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.Graywulf.Keystone
{
    internal static class Constants
    {
        public const string KeystoneAuthorityName = "Keystone";

        /// <summary>
        /// Header name for passing token for authentication
        /// </summary>
        public const string KeystoneXAuthTokenHeader = "X-Auth-Token";

        /// <summary>
        /// Header name for passing token subject to manipulation
        /// </summary>
        public const string KeystoneXSubjectTokenHeader = "X-Subject-Token";

        public const string KeystoneDefaultUri = "http://localhost:5000/";
        public const string KeystoneDefaultDomain = "default";
    }
}
