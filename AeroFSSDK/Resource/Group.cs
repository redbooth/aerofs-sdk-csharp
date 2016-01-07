using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class Group
    {
        public GroupID ID { get; set; }
        public string Name { get; set; }
        public IEnumerable<GroupMember> Members { get; set; }
    }
}
