using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestDownloadFile : BaseFileTest
    {
        [TestMethod]
        public void ShouldDownloadFile()
        {
            var f = Encoding.ASCII.GetBytes(new string('x', 1048576));
            var fileContents = new MemoryStream(f);
            var parent = Client.CreateFolder(FolderID.Root, "ParentFolder").ID;
            var file = Client.CreateFile(parent, "DownloadFileTest").ID;

            var progress = Client.StartUpload(file, fileContents);

            while (!progress.EOFReached)
            {
                progress = Client.UploadContent(file, progress, fileContents);
            }
            Client.FinishUpload(file, progress);

            var downloadContents = Client.DownloadFile(file);

            fileContents.Seek(0, SeekOrigin.Begin);

            int b1 = downloadContents.ReadByte(), b2 = fileContents.ReadByte();
            do
            {
                Assert.AreEqual(b1, b2);
                b1 = downloadContents.ReadByte();
                b2 = fileContents.ReadByte();
            } while (b1 != -1 && b2 != -1);

        }
    }
}
