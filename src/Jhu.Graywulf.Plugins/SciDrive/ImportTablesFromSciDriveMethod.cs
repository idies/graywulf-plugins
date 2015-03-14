using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.Web.UI;
using Jhu.Graywulf.Jobs.ImportTables;

namespace Jhu.Graywulf.SciDrive
{
    public class ImportTablesFromSciDriveMethod : ImportTablesMethod
    {
        public override string ID
        {
            get { return "import_scidrive"; }
        }

        public override string Description
        {
            get { return "Import from SciDrive"; }
        }

        public override void RegisterVirtualPaths(EmbeddedVirtualPathProvider vpp)
        {
            vpp.RegisterVirtualPath(ImportTablesFromSciDriveForm.GetUrl(), GetResourceName(typeof(ImportTablesFromSciDriveForm), ".ascx"));
        }

        public override string GetForm()
        {
            return ImportTablesFromSciDriveForm.GetUrl();
        }
    }
}
