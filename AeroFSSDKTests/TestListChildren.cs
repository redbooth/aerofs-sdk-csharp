using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestListChildren : BaseAPITest
    {
        [TestMethod]
        public void ShouldReturnListOfRootChildren()
        {
            var fileIDs = new[]
            {
                Client.CreateFile(FolderID.Root, "file1"),
                Client.CreateFile(FolderID.Root, "file2"),
                Client.CreateFile(FolderID.Root, "file3"),
            }.Select(file => file.ID).ToSet();
            var folderIDs = new[]
            {
                Client.CreateFolder(FolderID.Root, "folder1"),
                Client.CreateFolder(FolderID.Root, "folder2"),
            }.Select(folder => folder.ID).ToSet();

            var children = Client.ListRoot();

            Assert.IsTrue(fileIDs.SetEquals(children.Files.Select(file => file.ID)));
            Assert.IsTrue(folderIDs.SetEquals(children.Folders.Select(folder => folder.ID)));
        }

        [TestMethod]
        public void ShouldReturnListOfChildren()
        {
            var parent = Client.CreateFolder(FolderID.Root, "TestListChildren").ID;

            var files = new[]
            {
                Client.CreateFile(parent, "file1"),
                Client.CreateFile(parent, "file2"),
            };

            var folders = new[]
            {
                Client.CreateFolder(parent, "folder1"),
            };

            Client.CreateFile(FolderID.Root, "file2");
            Client.CreateFolder(FolderID.Root, "folder2");

            var children = Client.ListChildren(parent);

            Assert.IsTrue(new HashSet<FileID>(files.Select(file => file.ID)).SetEquals(children.Files.Select(file => file.ID)));
            Assert.IsTrue(new HashSet<FolderID>(folders.Select(folder => folder.ID)).SetEquals(children.Folders.Select(folder => folder.ID)));
        }

        [TestMethod]
        public void ShouldReturnEmptyList()
        {
            var folderID = Client.CreateFolder(FolderID.Root, "folder1").ID;
            var children = Client.ListChildren(folderID);

            Assert.IsFalse(children.Files.Any());
            Assert.IsFalse(children.Folders.Any());
        }
    }
}
