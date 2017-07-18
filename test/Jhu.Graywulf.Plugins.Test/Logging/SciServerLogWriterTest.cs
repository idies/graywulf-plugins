using System;
using Jhu.Graywulf.AccessControl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jhu.Graywulf.Logging
{
    [TestClass]
    public class SciServerLogWriterTest : LogWriterTestBase
    {
        protected override LogWriterBase CreateLogWriter(bool isasync)
        {
            var w = SciServerLogWriter.Configuration.CreateLogWriter();
            w.IsAsync = isasync;
            w.SeverityMask = EventSeverity.All;
            w.SourceMask = EventSource.All;
            w.StatusMask = ExecutionStatus.All;
            return w;
        }

        [TestMethod]
        public void WriteEventTest()
        {
            WriteEventTestHelper(false);
        }

        [TestMethod]
        public void WriteAsyncEventTest()
        {
            WriteEventTestHelper(true);
        }

        [TestMethod]
        public void WriteErrorTest()
        {
            WriteErrorTestHelper(false);
        }

        [TestMethod]
        public void WriteAsyncErrorTest()
        {
            WriteErrorTestHelper(true);
        }
    }
}
