using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using AeroFSSDK.Tests.Properties;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestOAuth : BaseTest
    {
        protected AeroFSAuthAPI Client { get; set; }

        protected string ClientID = "a5837de6-dba8-482b-9c84-110d1cbeb9e0";
        protected string ClientSecret = "65271610-0ea6-4e6e-be93-b3d2f7420720";
        protected string RedirectUri = "http://blackhole";

        [TestInitialize]
        public void SetupTestBase()
        {
            SetupClient();
        }

        private void SetupClient()
        {
            string accessToken = (string)Settings.Default["AccessToken"];

            Client = AeroFSAuthClient.Create(new AeroFSClient.Configuration
            {
                // read this value from app.config
                HostName = (string)Settings.Default["HostName"],
                APIVersion = (string)Settings.Default["APIVersion"]
            }, new AeroFSAuthClient.AppCredentials
            {
                ClientID = ClientID,
                ClientSecret = ClientSecret,
                RedirectUri = RedirectUri
            });
        }

        [TestMethod]
        public void ShouldReturnCorrectLinkOneScope()
        {
            OAuthScope scope = OAuthScope.OrganizationAdmin;
            var uri = Client.GenerateAuthorizationUrl(scope);
            var expect =
                String.Format("{0}/authorize?response_type=code", (string)Settings.Default["HostName"]) +
                String.Format("&client_id={0}", ClientID) +
                String.Format("&redirect_uri={0}", RedirectUri) +
                String.Format("&scope={0}", "organization.admin");
            Assert.AreEqual(uri, expect);

            // Can use this to manually test OAuth flow
            Console.WriteLine(uri);
        }

        [TestMethod]
        public void ShouldReturnCorrectLinkFourScopes()
        {
            OAuthScope[] scopes = new OAuthScope[] { OAuthScope.FilesRead, OAuthScope.FilesWrite, OAuthScope.ACLInvitations, OAuthScope.UserPassword };
            var uri = Client.GenerateAuthorizationUrl(scopes);
            var expect =
                String.Format("{0}/authorize?response_type=code", (string)Settings.Default["HostName"]) +
                String.Format("&client_id={0}", ClientID) +
                String.Format("&redirect_uri={0}", RedirectUri) +
                String.Format("&scope={0}", "files.read,files.write,acl.invitations,user.password");
            Assert.AreEqual(uri, expect);

            // Can use this to manually test OAuth flow
            Console.WriteLine(uri); 
        }

        [TestMethod]
        public void ShouldReturnValidAccessToken()
        {
            // Insert authorization code here
            string code = "f826b1b8231749cfa24fb8a2188c26fa";
            //Console.WriteLine(Client.ExchangeAuthorizationCodeForAccessToken(code));
        }
    }
}
