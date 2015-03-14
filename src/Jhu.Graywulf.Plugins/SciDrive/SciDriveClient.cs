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

        public static Credentials GetCredentials()
        {
            var name = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            Keystone.Token token;

            // TODO: replace keystone token to a trust here

            if (Keystone.KeystoneTokenCache.Instance.TryGetValueByUserName(name, name, out token))
            {
                var credentials = new Credentials();

                var header = new AuthenticationHeader()
                {
                    Name = Web.Security.KeystoneAuthentication.Configuration.AuthTokenHeader,
                    Value = token.ID
                };


                credentials.AuthenticationHeaders.Add(header);

                return credentials;
            }
            else
            {
                throw new System.Security.SecurityException("Keystone token required");
            }
        }
    }
}
