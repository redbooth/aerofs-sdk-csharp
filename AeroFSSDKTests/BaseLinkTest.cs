using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class BaseLinkTest : BaseFileTest
    {
        // Work in progress.
        /*
        [TestInitialize]
        public void DeleteAllLinks()
        {
            var shareID = Client.GetFolder(FolderID.Root).ID.ShareID;

            foreach (var link in Client.ListLinks(shareID))
            {
                Client.DeleteLink(shareID, link.Key);
            }
        }
        */
    }
}
