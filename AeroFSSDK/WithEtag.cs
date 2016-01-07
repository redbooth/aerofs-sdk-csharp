using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public interface WithETag
    {
        string ETag { get; set; }
    }
}
