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
            get { return new Uri(uri.Text, UriKind.RelativeOrAbsolute); }
            set { uri.Text = value.OriginalString; }
        }

        public Credentials Credentials
        {
            get
            {
                // TODO: add keystone header here
                return null;
            }
            set { }
        }
    }
}