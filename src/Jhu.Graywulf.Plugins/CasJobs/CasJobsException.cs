using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Jhu.Graywulf.SimpleRestClient;

namespace Jhu.Graywulf.CasJobs
{
    [Serializable]
    public class CasJobsException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public string ErrorType { get; set; }
        public string LogMessageID { get; set; }

        public CasJobsException()
        {
        }

        public CasJobsException(string message)
            : base(message)
        {
        }

        public CasJobsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CasJobsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
