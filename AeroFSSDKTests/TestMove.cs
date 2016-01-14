using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestMove : BaseFileTest
    {
        [TestMethod]
        public void ShouldRenameFolder()
        {
            var newName = "NewTestFolder";
            var folder = Client.CreateFolder(FolderID.Root, "TestFolder");
            var renamed = Client.MoveFolder(folder.ID, FolderID.Root, newName);
            var child = Client.ListChildren(FolderID.Root).Folders.First();

            Assert.AreEqual(renamed.ID, child.ID);
            Assert.AreEqual(newName, child.Name);
            Assert.AreEqual(renamed.IsShared, child.IsShared);
        }

        [TestMethod]
        public void ShouldMoveFolder()
        {
            var folderName = "TestFolder";
            var parent = Client.CreateFolder(FolderID.Root, "Parent");
            var folder = Client.CreateFolder(FolderID.Root, folderName);
            var moved = Client.MoveFolder(folder.ID, parent.ID, folderName);
            var child = Client.ListChildren(parent.ID).Folders.First();

            Assert.AreEqual(moved.ID, child.ID);
            Assert.AreEqual(folderName, child.Name);
            Assert.AreEqual(moved.IsShared, child.IsShared);
        }

        [TestMethod]
        public void ShouldNotMoveFolder()
        {
            var folderName = "TestFolder";
            var parent = Client.CreateFolder(FolderID.Root, "Parent");
            var folder = Client.CreateFolder(FolderID.Root, folderName);

            var moved = Client.MoveFolder(folder.ID, FolderID.Root, "NewName");

            ExpectError(HttpStatusCode.PreconditionFailed,
                () => Client.MoveFolder(folder.ID, parent.ID, folderName, new string[] { folder.ETag }));
        }

        [TestMethod]
        public void ShouldRenameFile()
        {
            var newName = "NewTestFile";
            var file = Client.CreateFile(FolderID.Root, "TestFile");
            var renamed = Client.MoveFile(file.ID, FolderID.Root, newName);
            var child = Client.ListChildren(FolderID.Root).Files.First();

            Assert.AreEqual(renamed.ID, child.ID);
            Assert.AreEqual(newName, child.Name);
            Assert.AreEqual(renamed.LastModified, child.LastModified);
        }

        [TestMethod]
        public void ShouldMoveFile()
        {
            var fileName = "TestFolder";
            var parent = Client.CreateFolder(FolderID.Root, "Parent");
            var file = Client.CreateFile(FolderID.Root, fileName);
            var moved = Client.MoveFile(file.ID, parent.ID, fileName);
            var child = Client.ListChildren(parent.ID).Files.First();

            Assert.AreEqual(moved.ID, child.ID);
            Assert.AreEqual(fileName, child.Name);
            Assert.AreEqual(moved.LastModified, child.LastModified);
        }

        [TestMethod]
        public void ShouldNotMoveFile()
        {
            var fileName = "TestFile";
            var parent = Client.CreateFolder(FolderID.Root, "Parent");
            var file = Client.CreateFile(FolderID.Root, fileName);

            var progress = Client.StartUpload(file.ID, new MemoryStream(Encoding.ASCII.GetBytes(new string('x', 1048576))));
            var etag = Client.FinishUpload(file.ID, progress);

            Client.MoveFile(file.ID, FolderID.Root, "NewName");

            ExpectError(HttpStatusCode.PreconditionFailed,
                () => Client.MoveFile(file.ID, parent.ID, fileName, new string[] { "suck" }));
        }
    }
}
