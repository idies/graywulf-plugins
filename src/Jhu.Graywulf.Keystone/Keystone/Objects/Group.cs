using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Jhu.Graywulf.Keystone
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Group : Entity
    {
        [JsonProperty("domain_id")]
        public string DomainID { get; set; }
    }
}
