using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.Web.UI;
using Jhu.Graywulf.Jobs.ExportTables;

namespace Jhu.Graywulf.SciDrive
{
    public class ExportTablesToSciDriveMethod : ExportTablesMethod
    {
        public override string ID
        {
            get { return "export_scidrive"; }
        }

        public override string Description
        {
            get { return "Export to SciDrive"; }
        }

        public override void RegisterVirtualPaths(EmbeddedVirtualPathProvider vpp)
        {
            vpp.RegisterVirtualPath(SciDriveForm.GetUrl(), GetResourceName(typeof(SciDriveForm), ".ascx"));
        }

        public override string GetForm()
        {
            return SciDriveForm.GetUrl();
        }
    }
}
