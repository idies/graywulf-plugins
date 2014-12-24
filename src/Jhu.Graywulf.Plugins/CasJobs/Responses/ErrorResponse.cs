using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Jhu.Graywulf.CasJobs
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class ErrorResponse
    {
        [JsonProperty("Error Code")]
        public int StatusCode { get; set; }

        [JsonProperty("Error Type")]
        public string ErrorType { get; set; }

        [JsonProperty("Error Message")]
        public string Message { get; set; }

        [JsonProperty("LogMessageID")]
        public string LogMessageID { get; set; }
    }
}