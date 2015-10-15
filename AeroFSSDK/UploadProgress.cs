using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class UploadProgress
    {
        public UploadID UploadID { get; set; }
        public long BytesUploaded { get; set; }
        public bool EOFReached { get; set; }
    }
}
