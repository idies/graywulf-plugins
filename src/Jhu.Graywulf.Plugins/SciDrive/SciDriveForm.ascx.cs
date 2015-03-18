using System;
using System.Web.UI;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Jobs.ImportTables;
using Jhu.Graywulf.Jobs.ExportTables;
using Jhu.Graywulf.Format;
using Jhu.Graywulf.IO;
using Jhu.Graywulf.IO.Tasks;

namespace Jhu.Graywulf.SciDrive
{
    public partial class SciDriveForm : UserControl, IImportTablesForm, IExportTablesForm
    {
        public static string GetUrl()
        {
            return "~/SciDrive/SciDriveForm.ascx";
        }

        public Uri Uri
        {
            get
            {
                return SciDriveClient.GetFileGetUri(new Uri(uri.Text, UriKind.Relative));
            }
            set
            {
                uri.Text = SciDriveClient.GetFilePath(value).ToString();
            }
        }

        public Credentials Credentials
        {
            get
            {
                // SciDrive credentials will be set by the job factory
                return null;
            }
            set { }
        }
    }
}