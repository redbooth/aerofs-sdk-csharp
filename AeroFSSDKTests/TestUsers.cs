using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestUsers : BaseAPITest
    {
        // README: populate these values to match an existing user
        //   before running this test.
        private string TestEmail { get; set; } = "";
        private string FirstName { get; set; } = "";
        private string LastName { get; set; } = "";

        [TestMethod]
        public void ShouldGetUserInfo()
        {
            Client.GetUserInfo(TestEmail);
        }
    }
}
