using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class Invitation
    {
        public ShareID SharedFolderID { get; set; }
        public string SharedFolderName { get; set; }
        public string InvitedBy { get; set; }
        public IEnumerable<Permission> Permissions { get; set; }
    }
}
