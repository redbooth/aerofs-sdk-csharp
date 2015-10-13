using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class File
    {
        public FileID ID { get; set; }
        public string Name { get; set; }
        public FolderID Parent { get; set; }
        public DateTime? LastModified { get; set; }
        public long? Size { get; set; }
        public string MimeType { get; set; }
        public string ETag { get; set; }
        public ParentPath Path { get; set; }
    }
}
