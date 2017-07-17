using System;
using Jhu.Graywulf.AccessControl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jhu.Graywulf.Logging
{
    [TestClass]
    public class SciServerLogWriterTest
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            Logging.LoggingContext.Current.StartLogger(Logging.EventSource.Test, false);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            Logging.LoggingContext.Current.StopLogger();
        }

        [TestMethod]
        public void WriteEventTest()
        {
            var guid = new Guid("413738E0-1111-1111-2222-D86E6A1AD098");

            var e = new Logging.Event()
            {
                BookmarkGuid = Guid.NewGuid(),
                Client = "127.0.0.1",
                Server = Environment.MachineName,
                ContextGuid = Guid.Empty,
                DateTime = DateTime.Now,
                ExecutionStatus = ExecutionStatus.Closed,
                JobGuid = guid,
                JobName = "job_name",
                Message = "This is a unit test.",
                Operation = "Jhu.Graywulf.Logging.SciServerLogWriterTest.WriteEventTest",
                Order = 0,
                Principal = new GraywulfPrincipal(
                    new GraywulfIdentity()
                    {
                        Evidence = guid,
                        SessionId = guid.ToString(),
                    }),
                Request = "GET /test",
                SessionGuid = guid,
                Severity = EventSeverity.Operation,
                Source = EventSource.Test,
                TaskName = "test_task",
                UserName = "test",
                UserGuid = guid,
            };

            e.UserData.Add("test_key", "test_value");

            Logging.LoggingContext.Current.WriteEvent(e);
        }

        private void WriteErrorTestHelper()
        {
            throw new Exception("Test exception");
        }

        [TestMethod]
        public void WriteErrorTest()
        {
            var guid = new Guid("413738E0-1111-1111-2222-D86E6A1AD098");

            try
            {
                WriteErrorTestHelper();
            }
            catch (Exception ex)
            {
                var e = Logging.LoggingContext.Current.CreateEvent(EventSeverity.Error, EventSource.Test, null, null, ex, null);

                e.BookmarkGuid = Guid.NewGuid();
                e.Client = "127.0.0.1";
                e.Server = Environment.MachineName;
                e.ContextGuid = Guid.Empty;
                e.DateTime = DateTime.Now;
                e.ExecutionStatus = ExecutionStatus.Closed;
                e.JobGuid = guid;
                e.JobName = "job_name";
                e.Order = 0;
                e.Principal = new GraywulfPrincipal(
                    new GraywulfIdentity()
                    {
                        Evidence = guid,
                        SessionId = guid.ToString(),
                    });
                e.Request = "GET /test";
                e.SessionGuid = guid;
                e.Source = EventSource.Test;
                e.TaskName = "test_task";
                e.UserName = "test";
                e.UserGuid = guid;

                e.UserData.Add("test_key", "test_value");

                Logging.LoggingContext.Current.RecordEvent(e);
            }
        }
    }
}
