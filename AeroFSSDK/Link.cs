using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class Link
    {
        public LinkID Key { get; set; }
        public ShareID ShareID { get { return ObjectID.ShareID; } }

        /// <remark>
        /// We have no information on whether the target is a folder or a file.
        /// </remark>
        public ObjectID ObjectID { get; set; }

        public string Token { get; set; }
        public string CreatedBy { get; set; }
        public bool RequireLogin { get; set; }
        public bool HasPassword { get; set; }

        /// <remark>
        /// Seconds in server time until the link's expiry.
        /// </remark>
        public long Expires { get; set; }
    }
}
