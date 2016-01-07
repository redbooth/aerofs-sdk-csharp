using AeroFSSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AeroFSSDKDemoApp.Models
{
    public class HomeModel
    {
        public Folder CurFolder { get; set; }
        public IList<Folder> Folders { get; set; }
        public IList<File> Files { get; set; }
        public ParentPath Path { get; set; }
    }
}