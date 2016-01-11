using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestCreateFile : BaseFileTest
    {
        [TestMethod]
        public void ShouldCreateFileWithNoContents()
        {
            var parent = Client.CreateFolder(FolderID.Root, "TestCreateFileParent").ID;
            var name = "TestCreateFile";
            var fileID = Client.CreateFile(parent, name).ID;
            var file = Client.GetFile(fileID);

            Assert.AreEqual(name, file.Name);
            Assert.AreEqual(parent, file.Parent);
            Assert.IsNull(file.LastModified); // because we haven't uploaded any content
            Assert.IsNull(file.Size); // because we haven't uploaded any content
        }

        [TestMethod]
        public void ShouldCreateFileWithEmptyContents()
        {
            var parent = Client.CreateFolder(FolderID.Root, "TestCreateFileParent").ID;
            var name = "TestCreateFile";
            var fileID = Client.CreateFile(parent, name).ID;
            var progress = Client.StartUpload(fileID, Stream.Null);
            var etag = Client.FinishUpload(fileID, progress);
            var file = Client.GetFile(fileID);

            Assert.AreEqual(name, file.Name);
            Assert.AreEqual(parent, file.Parent);
            Assert.AreEqual(etag, file.ETag);
            Assert.IsNotNull(file.LastModified);
            Assert.AreEqual(0, file.Size);
        }
    }
}
