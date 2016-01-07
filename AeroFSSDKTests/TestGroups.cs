using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestGroups : BaseFileTest
    {
        string TestEmail1 = "testuser1+testuser@aerofs.com";
        string TestEmail2 = "testuser2+testuser@aerofs.com";
        string FirstName = "Test";
        string LastName = "User";
        string GroupName = "Test Group";
        GroupID GroupID { get; set; }

        [TestInitialize]
        public void SetupGroup()
        {
            var groups = OrgAdminClient.ListGroups().Groups;
            if (groups.Where(g => g.Name == GroupName).Count() != 0)
            {
                OrgAdminClient.DeleteGroup(groups.Where(g => g.Name == GroupName).Single().ID);
            }
            OrgAdminClient.CreateUser(TestEmail1, FirstName, LastName);
            OrgAdminClient.CreateUser(TestEmail2, FirstName, LastName);
            var group = OrgAdminClient.CreateGroup(GroupName);
            GroupID = group.ID;
            OrgAdminClient.AddMemberToGroup(group.ID, TestEmail1);
            OrgAdminClient.AddMemberToGroup(group.ID, TestEmail2);
        }

        [TestCleanup]
        public void CleanupGroups()
        {
            var groups = OrgAdminClient.ListGroups().Groups;
            OrgAdminClient.DeleteGroup(groups.Where(g => g.Name == GroupName).Single().ID);
            OrgAdminClient.DeleteUser(TestEmail1);
            OrgAdminClient.DeleteUser(TestEmail2);
        }

        [TestMethod]
        public void ShouldListGroups()
        {
            var groups = OrgAdminClient.ListGroups(numResults: 1000).Groups;
            Assert.AreEqual(1, groups.Where(g => g.Name == GroupName).Count());
        }

        [TestMethod]
        public void ShouldCreateGroup()
        {
            var group = OrgAdminClient.CreateGroup("Test");
            Assert.AreEqual("Test", OrgAdminClient.GetGroup(group.ID).Name);
            OrgAdminClient.DeleteGroup(group.ID);
        }

        [TestMethod]
        public void ShouldGetGroup()
        {
            var group = OrgAdminClient.GetGroup(GroupID);
            Assert.AreEqual(GroupName, group.Name);
            Assert.IsTrue(group.Members.Select(g => g.Email).Contains(TestEmail1));
            Assert.IsTrue(group.Members.Select(g => g.Email).Contains(TestEmail2));
        }

        [TestMethod]
        public void ShouldDeleteGroup()
        {
            var group = OrgAdminClient.CreateGroup("Test");
            OrgAdminClient.DeleteGroup(group.ID);
            ExpectError(System.Net.HttpStatusCode.NotFound, () => OrgAdminClient.GetGroup(group.ID));
        }
    }
}
