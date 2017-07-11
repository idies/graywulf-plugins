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
            Logger.Instance.Start(Logging.EventSource.Test, false);

            Logger.Instance.LogOperation(Logging.EventSource.Test, "TestMessage", "WriteEventTest");

            Logger.Instance.Stop();
        }
    }
}
