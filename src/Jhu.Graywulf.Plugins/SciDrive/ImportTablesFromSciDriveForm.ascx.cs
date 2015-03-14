using System;
using System.Web.UI;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Jobs.ImportTables;
using Jhu.Graywulf.Format;
using Jhu.Graywulf.IO;
using Jhu.Graywulf.IO.Tasks;

namespace Jhu.Graywulf.SciDrive
{
    public partial class ImportTablesFromSciDriveForm : UserControl, IImportTablesForm
    {
        public static string GetUrl()
        {
            return "~/Jobs/ImportTables/ImportTablesFromSciDriveForm.ascx";
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
                return SciDriveClient.GetCredentials();
            }
            set { }
        }
    }
}