using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class UserPage
    {
        public IList<User> Users { get; set; }
        public bool HasMore { get; set; }
    }
}
