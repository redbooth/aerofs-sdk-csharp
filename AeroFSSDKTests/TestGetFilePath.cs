using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestGetFilePath : BaseAPITest
    {
        [TestMethod]
        public void ShouldReturnFilePath()
        {
            var root = Client.GetFolder(FolderID.Root).ID;
            var parent = Client.CreateFolder(root, "TestGetFileParent").ID;
            var name = "TestGetFile";
            var file = Client.CreateFile(parent, name).ID;
            var path = new[] { root, parent };

            var filePath = Client.GetFilePath(file);

            Assert.IsNotNull(filePath);
            Assert.IsTrue(path.SequenceEqual(filePath.Folders.Select(folder => folder.ID)));
        }
    }
}
