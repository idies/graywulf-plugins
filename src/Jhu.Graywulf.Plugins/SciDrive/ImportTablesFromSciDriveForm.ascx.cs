using System;
using System.Web.UI;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.IO.Jobs.ImportTables;
using Jhu.Graywulf.IO.Jobs.ExportTables;
using Jhu.Graywulf.Format;
using Jhu.Graywulf.IO;
using Jhu.Graywulf.IO.Tasks;

namespace Jhu.Graywulf.SciDrive
{
    public partial class ImportTablesFromSciDriveForm : UserControl, IImportTablesForm
    {
        public static string GetUrl()
        {
            return "~/SciDrive/ImportTablesFromSciDriveForm.ascx";
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

        protected void Page_Init(object sender, EventArgs e)
        {
            uriFormatValidator.ValidationExpression = Jhu.Graywulf.IO.Constants.UrlPathPattern;
        }
    }
}