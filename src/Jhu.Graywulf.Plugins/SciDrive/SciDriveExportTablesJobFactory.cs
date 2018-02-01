using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.IO.Jobs.ExportTables;


namespace Jhu.Graywulf.SciDrive
{
    public class SciDriveExportTablesJobFactory : ExportTablesJobFactory
    {
        public override IEnumerable<IO.Jobs.ExportTables.ExportTablesMethod> EnumerateMethods()
        {
            foreach (var method in base.EnumerateMethods())
            {
                yield return method;
            }

            yield return new ExportTablesToSciDriveMethod();
        }

        public override ExportTablesParameters CreateParameters(Registry.Federation federation, Uri uri, IO.Credentials credentials, IO.Tasks.SourceTable source, string mimeType)
        {
            // Intercept scidrive URIs and modify credentials

            if (SciDriveClient.IsUriSciDrive(uri))
            {
                credentials = credentials ?? new IO.Credentials();
                SciDriveClient.SetAuthenticationHeaders(credentials);
            }
            
            return base.CreateParameters(federation, uri, credentials, source, mimeType);
        }
    }
}
