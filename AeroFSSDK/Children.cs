using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class Children
    {
        public IEnumerable<Folder> Folders { get; set; }
        public IEnumerable<File> Files { get; set; }
    }
}
