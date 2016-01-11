using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestGetFile : BaseFileTest
    {
        [TestMethod]
        public void ShouldReturnFile()
        {
            var name = "TestGetFile";
            var fileID = Client.CreateFile(FolderID.Root, name).ID;
            var file = Client.GetFile(fileID);

            Assert.AreEqual(name, file.Name);
        }

        [TestMethod]
        public void ShouldReturnFileWithPath()
        {
            var root = Client.GetFolder(FolderID.Root).ID;
            var parent = Client.CreateFolder(root, "TestGetFileParent").ID;
            var name = "TestGetFile";
            var fileID = Client.CreateFile(parent, name).ID;
            var file = Client.GetFile(fileID, GetFileFields.Path);
            var path = new[] { root, parent };

            Assert.AreEqual(name, file.Name);
            Assert.AreEqual(parent, file.Parent);
            Assert.IsNotNull(file.Path);
            Assert.IsTrue(path.SequenceEqual(file.Path.Folders.Select(folder => folder.ID)));
        }
    }
}
