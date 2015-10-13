using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestDeleteFile : BaseAPITest
    {
        [TestMethod]
        public void ShouldDeleteFile()
        {
            var fileID = Client.CreateFile(FolderID.Root, "TestDeleteFile").ID;
            Client.GetFile(fileID);
            Client.DeleteFile(fileID);

            ExpectError(HttpStatusCode.NotFound, () => Client.GetFile(fileID));
        }

        [TestMethod]
        public void ShouldThrowOnDeletingNonExistantFile()
        {
            var fileID = Client.CreateFile(FolderID.Root, "TestDeleteFile").ID;
            Client.DeleteFile(fileID);

            ExpectError(HttpStatusCode.NotFound, () => Client.DeleteFile(fileID));
        }
    }
}
