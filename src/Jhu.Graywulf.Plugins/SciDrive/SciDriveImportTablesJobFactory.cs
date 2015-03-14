using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.Jobs.ImportTables;

namespace Jhu.Graywulf.SciDrive
{
    public class SciDriveImportTablesJobFactory : ImportTablesJobFactory
    {
        public override IEnumerable<Jobs.ImportTables.ImportTablesMethod> EnumerateMethods()
        {
            foreach (var method in base.EnumerateMethods())
            {
                yield return method;
            }

            yield return new ImportTablesFromSciDriveMethod();
        }

        public override ImportTablesParameters CreateParameters(Registry.Federation federation, Uri uri, IO.Credentials credentials, Format.DataFileBase source, IO.Tasks.DestinationTable destination)
        {
            // TODO: intercept scidrive URIs and modify credentials

            return base.CreateParameters(federation, uri, credentials, source, destination);
        }
    }
}
