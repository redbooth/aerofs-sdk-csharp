using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestGetFolder : BaseAPITest
    {
        [TestMethod]
        public void ShouldReturnFolder()
        {
            var parent = Client.CreateFolder(FolderID.Root, "TestGetFolderParent").ID;
            var name = "TestGetFolder";
            var folderID = Client.CreateFolder(parent, name).ID;
            var folder = Client.GetFolder(folderID);

            Assert.AreEqual(name, folder.Name);
            Assert.AreEqual(parent, folder.Parent);
            Assert.AreEqual(false, folder.IsShared);
            Assert.IsNull(folder.ShareID);
        }

        [TestMethod]
        public void ShouldReturnFolderWithNoChildren()
        {
            var name = "TestGetFolder";
            var folderID = Client.CreateFolder(FolderID.Root, name).ID;
            var folder = Client.GetFolder(folderID, GetFolderFields.Children);

            Assert.AreEqual(name, folder.Name);
            Assert.IsNotNull(folder.Children);
            Assert.IsFalse(folder.Children.Files.Any());
            Assert.IsFalse(folder.Children.Folders.Any());
        }

        [TestMethod]
        public void ShouldReturnFolderWithPathAndChildren()
        {
            var root = Client.GetFolder(FolderID.Root).ID;
            var parent = Client.CreateFolder(root, "TestGetFolderParent").ID;
            var name = "TestGetFolder";
            var folderID = Client.CreateFolder(parent, name).ID;
            var path = new[] { root, parent }; 
            var files = new[]
            {
                Client.CreateFile(folderID, "file1").ID,
                Client.CreateFile(folderID, "file2").ID,
            };
            var folders = new[]
            {
                Client.CreateFolder(folderID, "folder1").ID,
            };

            var folder = Client.GetFolder(folderID, GetFolderFields.Path | GetFolderFields.Children);

            Assert.AreEqual(name, folder.Name);
            Assert.IsTrue(path.SequenceEqual(folder.Path.Folders.Select(f => f.ID)));
            Assert.IsTrue(files.SequenceEqual(folder.Children.Files.Select(f => f.ID)));
            Assert.IsTrue(folders.SequenceEqual(folder.Children.Folders.Select(f => f.ID)));
        }
    }
}
