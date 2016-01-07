using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class DeviceStatus
    {
        public bool? IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }
    }
}