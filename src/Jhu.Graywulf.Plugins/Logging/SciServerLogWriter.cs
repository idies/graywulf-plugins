using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json.Linq;
using Jhu.Graywulf.Logging;

namespace Jhu.Graywulf.Logging
{
    public class SciServerLogWriter : LogWriterBase
    {
        public static SciServerLogWriterConfiguration Configuration
        {
            get
            {
                return (SciServerLogWriterConfiguration)ConfigurationManager.GetSection("jhu.graywulf/logging/sciServer");
            }
        }

        private string applicatioName;
        private string messagingHost;
        private string exchangeName;
        private string databaseQueueName;
        private SciServer.Logging.Logger logger;

        public string ApplicationName
        {
            get { return applicatioName; }
            set { applicatioName = value; }
        }

        public string MessagingHost
        {
            get { return messagingHost; }
            set { messagingHost = value; }
        }

        public string ExchangeName
        {
            get { return exchangeName; }
            set { exchangeName = value; }
        }

        public string DatabaseQueueName
        {
            get { return databaseQueueName; }
            set { databaseQueueName = value; }
        }

        public SciServerLogWriter()
        {
            InitializeMembers();
        }

        private void InitializeMembers()
        {
            applicatioName = "Graywulf";
            messagingHost = null;
            exchangeName = null;
            databaseQueueName = null;
            logger = null;
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void Start()
        {
            logger = new SciServer.Logging.Logger()
            {
                ApplicationHost = Environment.MachineName.ToUpperInvariant(),
                ApplicationName = applicatioName,
                MessagingHost = messagingHost,
                ExchangeName = exchangeName,
                DatabaseQueueName = databaseQueueName,
                Enabled = true,
            };

            logger.Connect();
        }

        public override void Stop()
        {
            if (logger != null)
            {
                logger.Dispose();
                logger = null;
            }
        }

        protected override void OnWriteEvent(Event e)
        {
            var msg = logger.CreateSkyQueryMessage(null, e.UserGuid != Guid.Empty);

            msg.UserId = e.UserGuid == Guid.Empty ? null : e.UserGuid.ToString();
            msg.UserName = e.UserName;
            msg.TaskName = e.TaskName;
            msg.Time = e.DateTime;
            msg.Method = e.Operation;
            msg.Host = e.Server;
            msg.ClientIP = e.Client;

            if ((e.Source & (EventSource.Workflow | EventSource.Scheduler | EventSource.Job)) != 0)
            {
                msg.Application = "SkyQuery.Scheduler";
            }
            else if ((e.Source & EventSource.RemoteService) != 0)
            {
                msg.Application = "SkyQuery.RemoteService";
            }
            else if ((e.Source & EventSource.WebUI) != 0)
            {
                msg.Application = "SkyQuery.WebUI";
            }
            else if ((e.Source & EventSource.WebService) != 0)
            {
                msg.Application = "SkyQuery.WebService";
            }
            else if ((e.Source & EventSource.WebAdmin) != 0)
            {
                msg.Application = "SkyQuery.WebAdmin";
            }
            else
            {
                msg.Application = "SkyQuery";
            }
            

            if (e.BookmarkGuid != Guid.Empty)
            {
                msg.MessageId = e.BookmarkGuid;
            }

            msg.MessageType = SciServer.Logging.MessageType.SKYQUERY;

            var principal = e.Principal as Graywulf.AccessControl.GraywulfPrincipal;

            if (principal != null)
            {
                msg.UserToken = principal.Identity.Evidence.ToString();
                msg.UserName = principal.Identity.Name;
            }
            else
            {
                msg.UserToken = null;
                msg.UserName = null;
            }

            if (e.Severity == EventSeverity.Error)
            {
                msg.MessageBody = new SciServer.Logging.ExceptionMessageBody()
                {
                    ExceptionText = e.Message,
                    ExceptionType = e.ExceptionType,
                    StackTrace = e.ExceptionStackTrace,
                };
            }

            if (e.JobName != null)
            {
                msg.MessageBody.Properties.Add("JobID", e.JobName);
            }

            if (e.Message != null)
            {
                msg.MessageBody.Properties.Add("Action", e.Message);
            }

            foreach (var d in e.UserData)
            {
                msg.MessageBody.Properties.Add(d.Key, d.Value.ToString());
            }

            logger.SendMessage(msg);
        }
    }
}
