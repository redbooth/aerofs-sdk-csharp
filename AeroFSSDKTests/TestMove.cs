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
            var child = Client.ListChildren(FolderID.Root).Folders.Single();

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
            var child = Client.ListChildren(parent.ID).Folders.Single();

            Assert.AreEqual(moved.ID, child.ID);
            Assert.AreEqual(folderName, child.Name);
            Assert.AreEqual(moved.IsShared, child.IsShared);
        }

        [TestMethod]
        public void ShouldNotMoveFolderOldEtag()
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
            var child = Client.ListChildren(FolderID.Root).Files.Single();

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
            var child = Client.ListChildren(parent.ID).Files.Single();

            Assert.AreEqual(moved.ID, child.ID);
            Assert.AreEqual(fileName, child.Name);
            Assert.AreEqual(moved.LastModified, child.LastModified);
        }

        [TestMethod]
        public void ShouldNotMoveFileOldEtag()
        {
            var fileName = "TestFile";
            var parent = Client.CreateFolder(FolderID.Root, "Parent");
            var file = Client.CreateFile(FolderID.Root, fileName);

            Client.MoveFile(file.ID, FolderID.Root, "NewName");

            ExpectError(HttpStatusCode.PreconditionFailed,
                () => Client.MoveFile(file.ID, parent.ID, fileName, new string[] { file.ETag }));
        }
    }
}
