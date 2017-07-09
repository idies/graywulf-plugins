using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jhu.Graywulf.Logging;

namespace Jhu.Graywulf.Logging
{
    public class SciServerLogWriter : LogWriterBase
    {
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
            SciServer.Logging.Message msg;

            if (e.Severity == EventSeverity.Error)
            {
                msg = logger.CreateErrorMessage(e.Exception, null);
            }
            else
            {
                // *** TODO: should it be CreateSkyQueryMessage?
                msg = logger.CreateSkyQueryMessage(e.Message ?? "", false);

                msg.ClientIP = "0.0.0.0";
                //msg.Host = 
                //msg.MessageId =
                msg.MessageType = SciServer.Logging.MessageType.SKYQUERY;
                msg.Method = e.Operation;
                msg.TaskName = e.JobGuid.ToString();
                msg.UserId = e.UserGuid.ToString();
                //msg.UserName = 
                //msg.UserToken = 

                //msg.MessageBody =

                foreach (var d in e.UserData)
                {
                    msg.MessageBody.Properties.Add(d.Key, d.Value.ToString());
                }
            }

            logger.SendMessage(msg);
        }
    }
}
