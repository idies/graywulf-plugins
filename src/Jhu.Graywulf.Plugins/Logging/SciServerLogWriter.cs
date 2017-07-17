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
            var content = new JObject();

            msg.UserId = e.UserGuid == Guid.Empty ? null : e.UserGuid.ToString("n");
            msg.UserName = e.UserName;
            msg.TaskName = e.TaskName;
            msg.Time = e.DateTime;
            msg.Method = e.Operation;
            msg.Host = e.Server;
            msg.ClientIP = e.Client;
            msg.MessageId = e.BookmarkGuid != Guid.Empty ? e.BookmarkGuid : Guid.NewGuid();
            msg.MessageType = SciServer.Logging.MessageType.SKYQUERY;
            msg.Application = GetMessageApplication(e);

            var principal = e.Principal as Graywulf.AccessControl.GraywulfPrincipal;

            if (principal != null)
            {
                msg.UserToken = principal.Identity.Evidence?.ToString();
            }
            else
            {
                msg.UserToken = null;
            }

            if (e.JobName != null)
            {
                content.Add("job_name", e.JobName);
            }

            if (e.Request != null)
            {
                content.Add("request", e.Request);
            }

            foreach (var d in e.UserData)
            {
                content.Add(d.Key, d.Value.ToString());
            }
            
            if (e.Severity == EventSeverity.Error)
            {
                msg.MessageBody = new SciServer.Logging.ExceptionMessageBody()
                {
                    ExceptionText = e.Message,
                    ExceptionType = e.ExceptionType,
                    StackTrace = e.ExceptionStackTrace,
                    Content = content.ToString(Newtonsoft.Json.Formatting.None)
                };
            }
            else
            {
                if (e.Message != null)
                {
                    content.Add("message", e.Message);
                }

                msg.MessageBody = new SciServer.Logging.SkyQueryMessageBody
                {
                    DoShowInUserHistory = true,
                    Content = content.ToString(Newtonsoft.Json.Formatting.None)
                };
            }

            logger.SendMessage(msg);
        }

        private string GetMessageApplication(Event e)
        {
            var module = "";

            if ((e.Source & (EventSource.Workflow | EventSource.Scheduler | EventSource.Job)) != 0)
            {
                module = "Scheduler";
            }
            else if ((e.Source & EventSource.RemoteService) != 0)
            {
                module = "RemoteService";

            }
            else if ((e.Source & EventSource.WebUI) != 0)
            {
                module = "WebUI";

            }
            else if ((e.Source & EventSource.WebService) != 0)
            {
                module = "WebService";

            }
            else if ((e.Source & EventSource.WebAdmin) != 0)
            {
                module = "WebAdmin";

            }
            else
            {
                return Configuration.ApplicationName;
            }


            if (!String.IsNullOrWhiteSpace(module))
            {
                return Configuration.ApplicationName;
            }
            else
            {
                return Configuration.ApplicationName + "." + module;
            }
        }
    }
}
