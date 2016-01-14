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
                OrgAdminClient.DeleteInvitation(TestInvitee);
            }
            // Swallow error, invitation doesn't exist.
            catch (WebException) { }
        }

        [TestMethod]
        public void ShouldGetInviteeInfo()
        {
            OrgAdminClient.CreateInvitation(TestInvitee, TestUser);
            var invitee = OrgAdminClient.GetInviteeInfo(TestInvitee);

            Assert.AreEqual(TestInvitee, invitee.EmailTo);
            Assert.AreEqual(TestUser, invitee.EmailFrom);
            Assert.IsNotNull(invitee.SignUpCode);
        }

        [TestMethod]
        public void ShouldDeleteInvitation()
        {
            OrgAdminClient.CreateInvitation(TestInvitee, TestUser);
            OrgAdminClient.DeleteInvitation(TestInvitee);
            ExpectError(HttpStatusCode.NotFound, () => OrgAdminClient.GetInviteeInfo(TestInvitee));
        }
    }
}
