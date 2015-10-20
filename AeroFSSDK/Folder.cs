using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    /// <remarks>
    /// If a folder is not shared, then IsShared will be false and SID will
    /// be null.
    ///
    /// Both Path and Children are on-demand fields and may be missing in some
    /// responses. In which case, these fields will be null.
    /// </remarks>
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
