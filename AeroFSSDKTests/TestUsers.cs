using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestUsers : BaseUserTest
    {
        string Email { get; set; }
        string FirstName = "Test";
        string LastName = "User";

        [TestInitialize]
        public void setEmail()
        {
            Email = TestEmails[0];
        }

        [TestMethod]
        public void ShouldGetUserInfo()
        {
            OrgAdminClient.CreateUser(Email, FirstName, LastName);

            var user = OrgAdminClient.GetUserInfo(Email);
            Assert.AreEqual(FirstName, user.FirstName);
            Assert.AreEqual(LastName, user.LastName);
        }

        [TestMethod]
        public void ShouldUpdateUser()
        {
            OrgAdminClient.CreateUser(Email, FirstName, LastName);
            
            var updatedLastName = "Updated";
            OrgAdminClient.UpdateUser(Email, FirstName, updatedLastName);
            var user = OrgAdminClient.GetUserInfo(Email);
            Assert.AreEqual(FirstName, user.FirstName);
            Assert.AreEqual(updatedLastName, user.LastName);
        }

        [TestMethod]
        public void ShouldListAllUsers()
        {
            foreach (var email in TestEmails)
            {
                OrgAdminClient.CreateUser(email, "Test", "User");
            }

            var users = OrgAdminClient.GetAllUsers();

            foreach (var email in TestEmails)
            {
                Assert.IsTrue(users.Select(u => u.Email).Contains(email));
            }
        }

        [TestMethod]
        public void ShouldListSomeUsers()
        {
            foreach (var email in TestEmails)
            {
                OrgAdminClient.CreateUser(email, "Test", "User");
            }

            var users = OrgAdminClient.ListUsers(after: ":2").Users;

            foreach (var email in TestEmails)
            {
                Assert.IsTrue(users.Select(u => u.Email).Contains(email));
            }
            Assert.IsFalse(users.Select(u => u.Email).Contains(":2"));
        }

        [TestMethod]
        public void ShouldDeleteUser()
        {
            OrgAdminClient.CreateUser(Email, FirstName, LastName);
            OrgAdminClient.DeleteUser(Email);
            ExpectError(System.Net.HttpStatusCode.NotFound, () => OrgAdminClient.GetUserInfo(Email));
        }

        [TestMethod]
        public void ShouldCheckTwoFactorAuth()
        {
            OrgAdminClient.CreateUser(Email, FirstName, LastName);
            var twoFactorEnabled = OrgAdminClient.IsUserTwoFactorAuthEnabled(Email);
            Assert.AreEqual(false, twoFactorEnabled);
        }
    }
}
