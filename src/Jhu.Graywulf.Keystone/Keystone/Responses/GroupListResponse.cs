using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Jhu.Graywulf.Keystone
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class GroupListResponse : ListResponse
    {
        [JsonProperty("groups")]
        public Group[] Groups { get; set; }
    }
}
