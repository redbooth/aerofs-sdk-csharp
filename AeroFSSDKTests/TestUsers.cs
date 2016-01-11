using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestUsers : BaseUserTest
    {
        // README: populate these values to match an existing user
        //   before running this test.
        private string TestEmail { get; set; } = "daniel.cressman@aerofs.com";
        private string FirstName { get; set; } = "Daniel";
        private string LastName { get; set; } = "Cressman";

        [TestMethod]
        public void ShouldGetUserInfo()
        {
            var user = OrgAdminClient.GetUserInfo(TestEmail);
            Assert.AreEqual(user.FirstName, FirstName);
            Assert.AreEqual(user.LastName, LastName);
        }
    }
}
