using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class SFGroupMember
    {
        public SFGroupMemberID ID { get; set; }
        public string Name { get; set; }
        public IList<Permission> Permissions { get; set; }
    }
}