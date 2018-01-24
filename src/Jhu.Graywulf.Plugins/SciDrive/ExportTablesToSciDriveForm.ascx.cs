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
    public partial class ExportTablesToSciDriveForm : UserControl, IExportTablesForm
    {
        public static string GetUrl()
        {
            return "~/SciDrive/ExportTablesToSciDriveForm.ascx";
        }

        public Uri Uri
        {
            get
            {
                return SciDriveClient.GetFilePutUri(new Uri(uri.Text, UriKind.Relative));
            }
            set
            {
                uri.Text = SciDriveClient.GetFilePath(value).ToString();
            }
        }

        public Uri CustomizableUri
        {
            get { return new Uri(uri.Text, UriKind.Relative); }
            set { uri.Text = value.ToString(); }
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

        public void GenerateDefaultUri(string filename)
        {
            uri.Text = "first_container/" + filename;
        }
    }
}