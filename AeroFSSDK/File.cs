using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    /// <remarks>
    /// A file may have no content. In which case, both LastModified and Size
    /// fields will be null to indicate their absence in the response. The
    /// caller can use this to distinguish between an empty content of size 0
    /// versus having no content.
    ///
    /// Note that Path field is an on-demand field and may be missing in some
    /// responses.
    /// </remarks>
    public class File : WithETag
    {
        public FileID ID { get; set; }
        public string Name { get; set; }
        public FolderID Parent { get; set; }
        public DateTime? LastModified { get; set; }
        public long? Size { get; set; }
        public string MimeType { get; set; }
        public string ETag { get; set; }
        public ParentPath Path { get; set; }
        public ContentState ContentState { get; set; }
    }
}
