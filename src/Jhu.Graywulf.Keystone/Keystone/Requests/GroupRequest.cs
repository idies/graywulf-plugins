using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Jhu.Graywulf.SimpleRestClient;

namespace Jhu.Graywulf.Keystone
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class GroupRequest
    {
        [JsonProperty("group")]
        public Group Group { get; private set; }

        public static GroupRequest Create(Group group)
        {
            return new GroupRequest()
            {
                Group = group
            };
        }

        public static RestMessage<GroupRequest> CreateMessage(Group group)
        {
            return new RestMessage<GroupRequest>(Create(group));
        }
    }
}
