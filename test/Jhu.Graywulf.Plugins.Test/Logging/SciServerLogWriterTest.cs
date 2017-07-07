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

            var e = new Event()
            {
                EventSource = EventSource.Test,
                Message = "TestMessage",
                Operation = "WriteEventTest"
            };

            Logger.Instance.LogEvent(e);

            Logger.Instance.Stop();
        }
    }
}
