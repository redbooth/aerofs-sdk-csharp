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
        [TestInitialize]
        public void DeleteAllLinks()
        {
            var shareID = Client.GetFolder(FolderID.Root).ID.ShareID;

            Console.Out.WriteLine("shareID: " + shareID.Base);

            foreach (var link in Client.ListLinks(shareID))
            {
                Client.DeleteLink(shareID, link.Key);
            }
        }
    }
}
