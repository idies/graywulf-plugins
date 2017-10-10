using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jhu.Graywulf.SciDrive
{
    [TestClass]
    public class SciDriveClientTest
    {
        [TestMethod]
        public void GetFilePathTest()
        {
            var uri = SciDriveClient.GetFileGetUri(new Uri("first_container/test.dat", UriKind.Relative));
            Assert.AreEqual("first_container/test.dat", SciDriveClient.GetFilePath(uri).ToString());

            uri = SciDriveClient.GetFilePutUri(new Uri("first_container/test.dat", UriKind.Relative));
            Assert.AreEqual("first_container/test.dat", SciDriveClient.GetFilePath(uri).ToString());
        }
    }
}
