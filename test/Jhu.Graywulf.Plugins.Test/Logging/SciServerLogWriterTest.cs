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
            Logging.LoggingContext.Current.StartLogger(Logging.EventSource.Test, false);
            Logging.LoggingContext.Current.LogOperation(Logging.EventSource.Test, "TestMessage", "WriteEventTest");
            Logging.LoggingContext.Current.StopLogger();
        }
    }
}
