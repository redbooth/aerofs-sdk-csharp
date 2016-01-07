using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using System.Net;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestSharedFolders : BaseFileTest
    {
        [TestMethod]
        public void ShouldListSharedFolders()
        {
            var f1Name = "Shared1";
            var f2Name = "Shared2";
            var folder1 = Client.CreateSharedFolder(f1Name);
            var folder2 = Client.CreateSharedFolder(f2Name);

            var sfList = Client.ListSharedFolders(Settings.EmailForAccessToken).SharedFolders;
            var checkFolder1 = sfList.Where(sf => sf.ID.Equals(folder1.ID));
            var checkFolder2 = sfList.Where(sf => sf.ID.Equals(folder2.ID));
            Assert.AreNotEqual(0, checkFolder1.Count());
            Assert.AreNotEqual(0, checkFolder2.Count());
            Assert.AreEqual(f1Name, checkFolder1.First().Name);
            Assert.AreEqual(f2Name, checkFolder2.First().Name);
        }

        [TestMethod]
        public void ShouldListSharedFoldersWithEtag()
        {
            var f1Name = "Shared1";
            var f2Name = "Shared2";
            var folder1 = Client.CreateSharedFolder(f1Name);
            var folder2 = Client.CreateSharedFolder(f2Name);

            var sharedFolders = Client.ListSharedFolders(Settings.EmailForAccessToken);
            var checkSharedFolders = Client.ListSharedFolders(Settings.EmailForAccessToken, new string[] { sharedFolders.ETag });
            Assert.IsNull(checkSharedFolders);
        }

        [TestMethod]
        public void ShouldGetSharedFolder()
        {
            var folderName = "SharedFolder";
            var folder = Client.GetSharedFolder(Client.CreateSharedFolder(folderName).ID);
            Assert.AreEqual(folderName, folder.Name);
        }

        [TestMethod]
        public void ShouldGetSharedFolderWithEtag()
        {
            var folderName = "SharedFolder";
            var folder = Client.CreateSharedFolder(folderName);
            var checkFolder = Client.GetSharedFolder(folder.ID);
            Assert.IsNull(checkFolder);
        }

        [TestMethod]
        public void ShouldListOneSFMemberAfterCreate()
        {
            var sharedFolder = Client.CreateSharedFolder("SharedFolder");
            var members = Client.ListSFMembers(sharedFolder.ID).SFMembers;
            Assert.AreEqual(1, members.Count());
        }

        [TestMethod]
        public void ShouldGetSFMember()
        {
            var sharedFolder = Client.CreateSharedFolder("SharedFolder");
            var member = Client.GetSFMember(sharedFolder.ID, Settings.EmailForAccessToken);
            Assert.AreEqual(Settings.EmailForAccessToken, member.Email);
        }

        [TestMethod]
        public void ShouldAddSFMember()
        {
            var sharedFolder = Client.CreateSharedFolder("SharedFolder");
            var testEmail = "testuser+test@aerofs.com";
            var firstName = "Test";
            var lastName = "User";
            var user = OrgAdminClient.CreateUser(testEmail, firstName, lastName);

            var permissions = new Permission[] { Permission.Write };
            var sfMember = OrgAdminClient.AddSFMemberToSharedFolder(sharedFolder.ID, user.Email, permissions);

            OrgAdminClient.DeleteUser(testEmail);

            Assert.AreEqual(user.Email, sfMember.Email);
            Assert.AreEqual("Test", sfMember.FirstName);
            Assert.AreEqual("User", sfMember.LastName);
            Assert.IsTrue(permissions.SequenceEqual(sfMember.Permissions));
        }

        [TestMethod]
        public void ShouldChangeSFMemberPermissions()
        {
            var sharedFolder = Client.CreateSharedFolder("SharedFolder");
            var testEmail = "testuser+test@aerofs.com";
            var firstName = "Test";
            var lastName = "User";
            var user = OrgAdminClient.CreateUser(testEmail, firstName, lastName);

            var permissions = new Permission[] { Permission.Write, Permission.Manage };
            OrgAdminClient.AddSFMemberToSharedFolder(sharedFolder.ID, user.Email, permissions);
            var sfMember = OrgAdminClient.SetSFMemberPermissions(sharedFolder.ID, user.Email, new Permission[0]);

            OrgAdminClient.DeleteUser(testEmail);

            Assert.AreEqual(user.Email, sfMember.Email);
            Assert.AreEqual("Test", sfMember.FirstName);
            Assert.AreEqual("User", sfMember.LastName);
            Assert.AreEqual(0, sfMember.Permissions.Count());
        }

        public void ShouldRemoveSFMemberFromSharedFolder()
        {
            var sharedFolder = Client.CreateSharedFolder("SharedFolder");
            var testEmail = "testuser+test@aerofs.com";
            var firstName = "Test";
            var lastName = "User";
            var user = OrgAdminClient.CreateUser(testEmail, firstName, lastName);

            OrgAdminClient.AddSFMemberToSharedFolder(sharedFolder.ID, user.Email, new Permission[0]);
            OrgAdminClient.RemoveMemberFromSharedFolder(sharedFolder.ID, user.Email);

            ExpectError(HttpStatusCode.NotFound, () => OrgAdminClient.GetSFMember(sharedFolder.ID, user.Email));

            OrgAdminClient.DeleteUser(testEmail);
        }
    }
}
