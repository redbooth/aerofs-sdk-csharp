using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestInvitees : BaseAPITest
    {
        private string TestInvitee = "testinvitee+plus@aerofs.com";
        private string TestUser = "testuser+plus@aerofs.com";

        [TestInitialize]
        public void DeleteTestInvitee()
        {
            try
            {
                OrgAdminClient.DeleteInvitee(TestInvitee);
            }
            // Swallow error, invitation doesn't exist.
            catch (WebException) { }
        }

        [TestMethod]
        public void ShouldGetInviteeInfo()
        {
            OrgAdminClient.CreateInvitee(TestInvitee, TestUser);
            var invitee = OrgAdminClient.GetInviteeInfo(TestInvitee);

            Assert.AreEqual(TestInvitee, invitee.EmailTo);
            Assert.AreEqual(TestUser, invitee.EmailFrom);
            Assert.IsNotNull(invitee.SignUpCode);
        }

        [TestMethod]
        public void ShouldDeleteInvitee()
        {
            OrgAdminClient.CreateInvitee(TestInvitee, TestUser);
            OrgAdminClient.DeleteInvitee(TestInvitee);
            ExpectError(HttpStatusCode.NotFound, () => OrgAdminClient.GetInviteeInfo(TestInvitee));
        }
    }
}
