using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Jhu.Graywulf.Keystone
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Token
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("issued_at")]
        public DateTime IssuedAt { get; private set; }

        [JsonProperty("expires_at")]
        public DateTime ExpiresAt { get; private set; }

        [JsonProperty("extras")]
        public object Extras { get; private set; }

        [JsonProperty("methods")]
        public string[] Methods { get; private set; }

        [JsonProperty("audit_ids")]
        public string[] AuditIDs { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("project")]
        public Project Project { get; private set; }

        [JsonProperty("domain")]
        public Domain Domain { get; private set; }

        [JsonProperty("catalog")]
        public Service[] Catalog { get; set; }

        // TODO: add bind if necessary

        [JsonProperty("OS-TRUST:trust")]
        public Trust Trust { get; set; }
    }
}
