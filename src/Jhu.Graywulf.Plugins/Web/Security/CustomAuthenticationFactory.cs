using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.Graywulf.Web.Security
{
    public class CustomAuthenticationFactory : AuthenticationFactory
    {

        public CustomAuthenticationFactory()
        {
        }

        protected override IEnumerable<Authentication> CreateAuthentications()
        {
            foreach (var a in base.CreateAuthentications())
            {
                yield return a;
            }

            // OpenID
            foreach (OpenIDProviderSettings oid in OpenIDAuthentication.Configuration.OpenIDProviders)
            {
                yield return new OpenIDAuthentication(oid);
            }

            // Keystone
            if (KeystoneAuthentication.Configuration.IsEnabled)
            {
                yield return new KeystoneAuthentication();
            }
        }
    }
}
