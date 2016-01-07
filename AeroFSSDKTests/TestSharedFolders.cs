using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestSharedFolders : BaseFileTest
    {
        [TestMethod]
        public void ShouldShareFolder()
        {
            var folder = Client.CreateFolder(FolderID.Root, "TestFolder");
            Client.ShareFolder(folder.ID);
            var sharedFolder = Client.GetFolder(folder.ID);
            Assert.IsTrue(sharedFolder.IsShared);
        }

        [TestMethod]
        public void ShouldListSharedFolders()
        {
            var f1Name = "Shared1";
            var f2Name = "Shared2";
            var folder1 = Client.CreateSharedFolder(f1Name);
            var folder2 = Client.CreateSharedFolder(f2Name);

            var sfList = Client.ListSharedFolders(Settings.EmailForAccessToken).SharedFolders;
            var checkFolder1 = sfList.Where(sf => sf.ID.Equals(folder1.ID));
            var checkFolder2 = sfList.Where(sf => sf.ID.Equals(folder2.ID));
            Assert.AreNotEqual(0, checkFolder1.Count());
            Assert.AreNotEqual(0, checkFolder2.Count());
            Assert.AreEqual(f1Name, checkFolder1.First().Name);
            Assert.AreEqual(f2Name, checkFolder2.First().Name);
        }

        [TestMethod]
        public void ShouldListSharedFoldersWithEtag()
        {
            var f1Name = "Shared1";
            var f2Name = "Shared2";
            var folder1 = Client.CreateSharedFolder(f1Name);
            var folder2 = Client.CreateSharedFolder(f2Name);

            var sharedFolders = Client.ListSharedFolders(Settings.EmailForAccessToken);
            var checkSharedFolders = Client.ListSharedFolders(Settings.EmailForAccessToken, new string[] { sharedFolders.ETag });
            Console.WriteLine(sharedFolders.ETag);
            Console.WriteLine(checkSharedFolders.ETag);
            Assert.IsNull(checkSharedFolders);
        }

        [TestMethod]
        public void ShouldGetSharedFolder()
        {
            var folderName = "SharedFolder";
            var folder = Client.GetSharedFolder(Client.CreateSharedFolder(folderName).ID);
            Assert.AreEqual(folderName, folder.Name);
        }

        [TestMethod]
        public void ShouldGetSharedFolderWithEtag()
        {
            var folderName = "SharedFolder";
            var folder = Client.CreateSharedFolder(folderName);
            var checkFolder = Client.GetSharedFolder(folder.ID);
            Assert.IsNull(checkFolder);
        }
    }
}
