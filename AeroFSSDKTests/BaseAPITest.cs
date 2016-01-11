using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using AeroFSSDK.Impl;
using AeroFSSDK.Tests.Properties;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class BaseAPITest : BaseTest
    {
        protected AeroFSAPI Client { get; set; }

        // Need to provide values here to configure tests
        // The host name of the appliance against which you want to run tests
        // NOTE: tests will wipe the AeroFS folder for the user against which tests are run
        // e.g. "https://share.aerofs.com"
        private string HostName = "https://share.syncfs.com";

        // The target API version of the appliance, e.g. "1.4"
        private string APIVersion = "1.4";

        // A valid access token to run tests with
        // Can be obtained through Settings menu > API Access Tokens in the Appliance
        // Token should NOT have organization.admin scope (which is the case by default)
        // If tests are failing with a 503 HTTP status, check if this is the case
        // e.g. "9dk4955c0407459d97b2c833ce08dbe3"
        private string AccessToken = "9cf4955c0407459d97b2c833ce08dbe3";

        [TestInitialize]
        public void SetupTestBase()
        {
            SetupClient();
            DeleteAllFiles();
            DeleteAllLinks();
        }

        private void SetupClient()
        {
            Client = AeroFSClient.Create(AccessToken, new AeroFSClient.Configuration
            {
                HostName = HostName,
                APIVersion = APIVersion
            });
        }

        private void DeleteAllFiles()
        {
            var children = Client.ListRoot();

            foreach (var folder in children.Folders)
            {
                Client.DeleteFolder(folder.ID);
            }

            foreach (var file in children.Files)
            {
                Client.DeleteFile(file.ID);
            }
        }

        private void DeleteAllLinks()
        {
            var shareID = Client.GetFolder(FolderID.Root).ID.ShareID;

            Console.Out.WriteLine("shareID: " + shareID.Base);

            foreach (var link in Client.ListLinks(shareID))
            {
                Client.DeleteLink(shareID, link.Key);
            }
        }

        protected bool IsHttpError(WebException e, HttpStatusCode statusCode)
        {
            return e.Response is HttpWebResponse
                && (e.Response as HttpWebResponse).StatusCode == statusCode;
        }

        protected void ExpectError(HttpStatusCode statusCode, Action action)
        {
            try
            {
                action.Invoke();
                Assert.Fail();
            }
            catch (WebException e)
            {
                Assert.IsTrue(IsHttpError(e, statusCode));
            }
        }
    }
}
