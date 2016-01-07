using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    /// <remarks>
    /// If a folder is not shared, then IsShared will be false and ShareID will
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
        // FIXME: ShareID is overloaded as it can be extracted from ID in some cases
        public ShareID ShareID { get; set; }
        public ParentPath Path { get; set; }
        public Children Children { get; set; }
        public string ETag { get; set; }
    }
}
