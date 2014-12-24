using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Jhu.Graywulf.CasJobs
{
    [JsonObject(MemberSerialization.OptIn)]
    public class User
    {
        [JsonProperty]
        public string UserId { get; set; }

        [JsonProperty]
        public string FullName { get; set; }

        [JsonProperty]
        public string Email { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MyDBHost { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MyDBName { get; set; }

        [JsonProperty]
        public long? WebServicesId { get; set; }
    }
}
