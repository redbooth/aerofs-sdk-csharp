using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class SharedFolderList : WithETag
    {
        public IList<SharedFolder> SharedFolders { get; set; }
        public string ETag { get; set;}
    }
}