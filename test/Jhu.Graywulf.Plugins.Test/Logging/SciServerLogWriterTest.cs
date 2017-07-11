using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jhu.Graywulf.Logging
{
    [TestClass]
    public class SciServerLogWriterTest
    {
        [TestMethod]
        public void WriteEventTest()
        {
            Logger.Instance.Start(false);

            Logger.Instance.LogInfo("TestMessage", "WriteEventTest");

            Logger.Instance.Stop();
        }
    }
}
