using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class SharedFolder : WithETag
    {
        public ShareID ID { get; set; }
        public string Name { get; set; }
        public IList<SFMember> Members { get; set; }
        public IList<SFGroupMember> Groups { get; set; }
        public IList<SFPendingMember> Pending { get; set; }
        public bool IsExternal { get; set; }
        public IList<Permission> CallerEffectivePermissions { get; set; }
        public string ETag { get; set; }
    }
}
