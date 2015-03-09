using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

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
    }
}
