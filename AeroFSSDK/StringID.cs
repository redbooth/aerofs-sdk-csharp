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

    public class FileID : StringID { }
    public class FolderID : StringID
    {
        public static FolderID Root { get; } = new FolderID { Base = "root" };
    }

    public class ShareID : StringID { }
    public class UploadID : StringID { }
}