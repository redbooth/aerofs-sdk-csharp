using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestDeleteFolder : BaseFileTest
    {
        [TestMethod]
        public void ShouldDeleteFolder()
        {
            var folderID = Client.CreateFolder(FolderID.Root, "TestDeleteFolder").ID;
            Client.GetFolder(folderID);
            Client.DeleteFolder(folderID);

            ExpectError(HttpStatusCode.NotFound, () => Client.GetFolder(folderID));
        }

        [TestMethod]
        public void ShouldDeleteFolderRecursively()
        {
            var folderID = Client.CreateFolder(FolderID.Root, "TestDeleteFolder").ID;
            var childFile = Client.CreateFile(folderID, "ChildFile").ID;
            var childFolder = Client.CreateFolder(folderID, "ChildFolder").ID;
            var grandchildFile = Client.CreateFile(childFolder, "GrandchildFile").ID;
            Client.DeleteFolder(folderID);

            ExpectError(HttpStatusCode.NotFound, () => Client.GetFolder(folderID));
            ExpectError(HttpStatusCode.NotFound, () => Client.GetFile(childFile));
            ExpectError(HttpStatusCode.NotFound, () => Client.GetFolder(childFolder));
            ExpectError(HttpStatusCode.NotFound, () => Client.GetFile(grandchildFile));
        }

        [TestMethod]
        public void ShouldThrowBadRequestOnDeleteRootFolder()
        {
            var root = Client.GetFolder(FolderID.Root).ID;

            ExpectError(HttpStatusCode.BadRequest, () => Client.DeleteFolder(FolderID.Root));
            ExpectError(HttpStatusCode.BadRequest, () => Client.DeleteFolder(root));
        }

        [TestMethod]
        public void ShouldThrowOnDeleteNonExistantFolder()
        {
            var folderID = Client.CreateFolder(FolderID.Root, "TestDeleteFolder").ID;
            Client.DeleteFolder(folderID);

            ExpectError(HttpStatusCode.NotFound, () => Client.DeleteFolder(folderID));
        }
    }
}
