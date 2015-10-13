using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class Folder
    {
        public FolderID ID { get; set; }
        public string Name { get; set; }
        public FolderID Parent { get; set; }
        public bool IsShared { get; set; }
        public ShareID SID { get; set; }
        public ParentPath Path { get; set; }
        public Children Children { get; set; }
    }
}
