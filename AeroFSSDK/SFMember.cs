using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class SFMember : WithETag
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IList<Permission> Permissions { get; set; }
        public string ETag { get; set; }
    }
}
