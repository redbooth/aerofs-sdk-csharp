using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class BaseUserTest : BaseAPITest
    {
        protected string[] TestEmails = { "testuser1+plusaddress@aerofs.com", "testuser2+plusaddress@aerofs.com", "testuser3+plusaddress@aerofs.com" };

        [TestInitialize]
        public void CleanUpTestUsers()
        {
            var users = OrgAdminClient.GetAllUsers();

            foreach (var user in users)
            {
                if (TestEmails.Contains(user.Email))
                {
                    OrgAdminClient.DeleteUser(user.Email);
                }
            }
        }
    }

    static class ClientExtensions
    {
        public static IList<User> GetAllUsers(this AeroFSAPI client)
        {
            var userPage = client.ListUsers();
            var users = userPage.Users;
            while (userPage.HasMore)
            {
                userPage = client.ListUsers();
                foreach (var user in userPage.Users)
                {
                    users.Add(user);
                }
            }

            return users;
        }
    }
}
