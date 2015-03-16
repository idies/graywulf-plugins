using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Jhu.Graywulf.IO;

namespace Jhu.Graywulf.SciDrive
{
    public class SciDriveClient
    {
        public static SciDriveConfiguration Configuration
        {
            get
            {
                return (SciDriveConfiguration)ConfigurationManager.GetSection("jhu.graywulf/sciDrive");
            }
        }

        private static bool IsUriSciDrive(Uri uri)
        {
            var c = StringComparer.InvariantCultureIgnoreCase;

            // If URI starts with scidrive:/ or when
            // http or https is used but host name equals to config
            if (c.Compare(uri.Scheme, Constants.UriSchemeSciDrive) == 0)
            {
                return true;
            }
            else if (c.Compare(uri.Scheme, Uri.UriSchemeHttp) == 0 ||
                     c.Compare(uri.Scheme, Uri.UriSchemeHttps) == 0)
            {
                // Compare host name with settings
                var uriHost = uri.Host;
                var configHost = SciDriveClient.Configuration.BaseUri.Host;

                // If host names match it is a SciDrive instance
                if (c.Compare(uriHost, configHost) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static Uri GetNormalizedUri(Uri uri)
        {
            var c = StringComparer.InvariantCultureIgnoreCase;

            if (c.Compare(uri.Scheme, Constants.UriSchemeSciDrive) == 0)
            {
                // If scidrive: is used as scheme, replace with URI from settings

                return Util.UriConverter.Combine(SciDriveClient.Configuration.BaseUri, uri.PathAndQuery);
            }
            else
            {
                // In any other case just use HTTP or HTTPS
                return uri;
            }
        }

        public static Uri GetFileGetUri(Uri path)
        {
            var uri = Configuration.BaseUri;
            uri = Util.UriConverter.Combine(uri, "1/files/dropbox/");
            uri = Util.UriConverter.Combine(uri, path);

            return uri;
        }

        public static Uri GetFilePutUri(Uri path)
        {
            var uri = Configuration.BaseUri;
            uri = Util.UriConverter.Combine(uri, "1/files_put/dropbox/");
            uri = Util.UriConverter.Combine(uri, path);

            return uri;
        }

        public static Uri GetFilePath(Uri uri)
        {
            return uri.MakeRelativeUri(Configuration.BaseUri);
        }

        public static void SetAuthenticationHeaders(Credentials credentials)
        {
            // TODO: use this once trust works
            //var header = CreateAuthenticationHeaderFromTrust();

            var header = CreateAuthenticationHeaderFromToken();
            credentials.AuthenticationHeaders.Add(header);
        }

        private static AuthenticationHeader CreateAuthenticationHeaderFromToken()
        {
            var name = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            Keystone.Token token;

            // TODO: replace keystone token to a trust here

            if (Keystone.KeystoneTokenCache.Instance.TryGetValueByUserName(name, name, out token))
            {
                var header = new AuthenticationHeader()
                {
                    Name = Web.Security.KeystoneAuthentication.Configuration.AuthTokenHeader,
                    Value = token.ID
                };

                return header;
            }
            else
            {
                throw new System.Security.SecurityException("Keystone token required");
            }
        }

        private static AuthenticationHeader CreateAuthenticationHeaderFromTrust()
        {
            var name = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            Keystone.Token token;

            // TODO: replace keystone token to a trust here

            if (Keystone.KeystoneTokenCache.Instance.TryGetValueByUserName(name, name, out token))
            {
                // Not we have a Keystone token but it needs to be exchanged for a longer-lived
                // trust for batch processing

                var ksclient = new Keystone.KeystoneClient()
                {
                    UserCredentials = new Keystone.KeystoneCredentials()
                    {
                        TokenID = token.ID
                    }
                };

                var trust = new Keystone.Trust()
                {
                    ExpiresAt = DateTime.UtcNow.AddHours(12),
                    ProjectID = token.Project.ID,
                    Impersonation = true,
                    TrustorUserID = token.User.ID,
                    //TrusteeUserID = token.User.ID,
                    TrusteeUserID = "9acc0f0db48f459389ce3cb0662c3f66",
                    RemainingUses = 5,
                    
                };

                trust = ksclient.Create(trust);

                var header = new AuthenticationHeader()
                {
                    Name = Web.Security.KeystoneAuthentication.Configuration.AuthTokenHeader,
                    Value = trust.ID
                };

                return header;
            }
            else
            {
                throw new System.Security.SecurityException("Keystone token required");
            }
        }
    }
}
