using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestUpload : BaseAPITest
    {
        [TestMethod]
        public void ShouldUploadFileWithContents()
        {
            var parent = Client.CreateFolder(FolderID.Root, "TestUploadParent").ID;
            var name = "TestUpload";
            var fileID = Client.CreateFile(parent, name).ID;

            UploadProgress progress;
            using (var content = CreateContent(10000))
            {
                progress = Client.StartUpload(fileID, content);

                while (!progress.EOFReached)
                {
                    progress = Client.UploadContent(fileID, progress, content);
                }
            }

            var etag = Client.FinishUpload(fileID, progress);
            var file = Client.GetFile(fileID);

            Assert.AreEqual(etag, file.ETag);
            Assert.AreEqual(10000, file.Size);
            Assert.IsNotNull(file.LastModified);
        }

        [TestMethod]
        public void ShouldResumeUploadWithNoContents()
        {
            var fileID = Client.CreateFile(FolderID.Root, "TestUpload").ID;
            var uploadID = Client.StartUpload(fileID, Stream.Null).UploadID;
            var progress = Client.ResumeUpload(fileID, uploadID);

            using (var content = CreateContent(3000))
            {
                while (!progress.EOFReached)
                {
                    progress = Client.UploadContent(fileID, progress, content);
                }
            }

            var etag = Client.FinishUpload(fileID, progress);
            var file = Client.GetFile(fileID);

            Assert.AreEqual(etag, file.ETag);
            Assert.AreEqual(3000, file.Size);
            Assert.IsNotNull(file.LastModified);
        }

        [TestMethod]
        public void ShouldResumeUploadWithSomeContents()
        {
            var fileID = Client.CreateFile(FolderID.Root, "TestUpload").ID;

            UploadProgress progress;
            // N.B. the content must be large enough that we don't finish uploading all contents in 3 chunks
            using (var content = CreateContent(30000))
            {
                progress = Client.StartUpload(fileID, content);
                progress = Client.UploadContent(fileID, progress, content);
                progress = Client.UploadContent(fileID, progress, content);
                progress = Client.ResumeUpload(fileID, progress.UploadID);

                while (!progress.EOFReached)
                {
                    progress = Client.UploadContent(fileID, progress, content);
                }
            }

            var etag = Client.FinishUpload(fileID, progress);
            var file = Client.GetFile(fileID);

            Assert.AreEqual(etag, file.ETag);
            Assert.AreEqual(30000, file.Size);
            Assert.IsNotNull(file.LastModified);
        }

        private Stream CreateContent(int contentSize)
        {
            return new MemoryStream(Encoding.ASCII.GetBytes(new string('y', contentSize)));
        }
    }
}
