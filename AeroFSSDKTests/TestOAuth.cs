using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestOAuth : BaseTest
    {
        protected AeroFSAuthAPI Client { get; set; }

        [TestInitialize]
        public void SetupTestBase()
        {
            SetupClient();
        }

        private void SetupClient()
        {
            Client = AeroFSAuthClient.Create(new AeroFSAuthClient.Configuration
            {
                // read this value from app.config
                HostName = Settings.HostName,
                ClientID = Settings.ClientID,
                ClientSecret = Settings.ClientSecret,
                RedirectUri = Settings.RedirectUri
            });
        }

        [TestMethod]
        public void ShouldReturnCorrectAuthUrlOneScope()
        {
            OAuthScope scope = OAuthScope.OrganizationAdmin;
            var uri = Client.GenerateAuthorizationUrl(new OAuthScope[] { scope });
            var expect =
                String.Format("{0}/authorize?response_type=code", Settings.HostName) +
                String.Format("&client_id={0}", Settings.ClientID) +
                String.Format("&redirect_uri={0}", Settings.RedirectUri) +
                String.Format("&scope={0}", "organization.admin");
            Assert.AreEqual(expect, uri);
        }

        [TestMethod]
        public void ShouldReturnCorrectAuthUrlFourScopes()
        {
            OAuthScope[] scopes = new OAuthScope[] { OAuthScope.FilesRead, OAuthScope.FilesWrite, OAuthScope.ACLInvitations, OAuthScope.UserPassword };
            var uri = Client.GenerateAuthorizationUrl(scopes);
            var expect =
                String.Format("{0}/authorize?response_type=code", Settings.HostName) +
                String.Format("&client_id={0}", Settings.ClientID) +
                String.Format("&redirect_uri={0}", Settings.RedirectUri) +
                String.Format("&scope={0}", "files.read,files.write,acl.invitations,user.password");
            Assert.AreEqual(expect, uri);
        }


        // These can be used for manual tests, but cannot yet be tested "automatically"
        [TestMethod]
        public void ShouldReturnValidAccessToken()
        {
            // Insert authorization code here
            string code = "";
            // Console.WriteLine(Client.ExchangeAuthorizationCodeForAccessToken(code));
        }

        // OAuth requires user interaction, so just revoking the provided token is annoying
        // Use if you wish
        [TestMethod]
        public void ShouldRevokeAccessToken()
        {
            // Insert token to revoke
            string token = "";
            // Client.RevokeAccessToken(token);
        }
    }
}
