using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class Device
    {
        public DeviceID ID { get; set; }
        public string Owner { get; set; }
        public string Name { get; set; }
        public string OSFamily { get; set; }
        public DateTime? InstallDate { get; set; }
    }
}
