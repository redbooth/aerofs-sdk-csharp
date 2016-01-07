using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class SFPendingMemberList
    {
        public IList<SFPendingMember> SFPendingMembers { get; set; }
        public string ETag { get; set; }
    }
}
