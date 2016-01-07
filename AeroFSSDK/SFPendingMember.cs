using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class SFPendingMember
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string InvitedBy { get; set; }
        public IList<Permission> Permissions { get; set; }
    }
}