using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jhu.Graywulf.Web.Services;
using Jhu.Graywulf.Scheduler;
using Jhu.Graywulf.RemoteService;
using Jhu.Graywulf.Registry;

namespace Jhu.Graywulf.SciDrive
{
    [TestClass]
    public sealed class ImportTest : Jhu.Graywulf.Web.Api.V1.ApiTestBase
    {
        [ClassInitialize]
        public new static void Initialize(TestContext context)
        {
            Jhu.Graywulf.Web.Api.V1.ImportTest.Initialize(context);
        }

        [ClassCleanup]
        public new static void CleanUp()
        {
            Jhu.Graywulf.Web.Api.V1.ImportTest.CleanUp();
        }

        protected override void ImportFileHelper(string uri, bool generateIdentityColumn)
        {
            var scuri =  SciDriveClient.GetFileGetUri(new Uri(uri, UriKind.RelativeOrAbsolute)).ToString();
            
            base.ImportFileHelper(scuri, generateIdentityColumn);
        }

        [TestMethod]
        public void ImportFileFromSciDriveTest()
        {
            ImportFileHelper("graywulf_io_test/csv_numbers.csv", false);
            ImportFileHelper("graywulf_io_test/csv_numbers.csv", true);
        }

        [TestMethod]
        public void ImportCompressedFromSciDriveTest()
        {
            ImportFileHelper("graywulf_io_test/csv_numbers.csv.gz", false);
            ImportFileHelper("graywulf_io_test/csv_numbers.csv.gz", true);
        }
        
        [TestMethod]
        public void ImportArchiveFromSciDriveTest()
        {
            ImportFileHelper("graywulf_io_test/csv_numbers.zip", false);
            ImportFileHelper("graywulf_io_test/csv_numbers.zip", true);
        }
    }
}
