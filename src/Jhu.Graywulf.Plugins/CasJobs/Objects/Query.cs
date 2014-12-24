using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Jhu.Graywulf.CasJobs
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Query
    {
        [JsonProperty("Query")]
        public string QueryText { get; set; }

        [JsonProperty]
        public string TaskName { get; set; }
    }
}
