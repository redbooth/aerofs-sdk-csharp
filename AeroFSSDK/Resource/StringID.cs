using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK
{
    public class StringID
    {
        public string Base { get; set; }

        public override string ToString()
        {
            return Base;
        }

        public override int GetHashCode()
        {
            return (Base ?? "").GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this == obj
                || (obj != null
                && GetType().Equals(obj.GetType())
                && (obj as StringID).Base == Base); // value equals since Base is a string
        }
    }

    public class ObjectID : StringID
    {
        // FIXME: while this hack technically works for special folders like root and approot,
        //   it leaves a bad taste nevertheless
        public ShareID ShareID
        {
            get { return new ShareID { Base = Base.Substring(0, 32) }; }
        }
    }

    public class FileID : ObjectID { }

    public class FolderID : ObjectID
    {
        public static FolderID Root { get; } = new FolderID { Base = "root" };
        public static FolderID AppData { get; } = new FolderID { Base = "appdata" };
    }

    public class ShareID : StringID { }
    public class UploadID : StringID { }
    public class LinkID : StringID { }
    public class SharedFolderID : StringID { }
    public class GroupID : StringID { }
    public class DeviceID : StringID { }
}