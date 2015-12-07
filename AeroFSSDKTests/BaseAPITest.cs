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

        [TestInitialize]
        public void SetupTestBase()
        {
            SetupClient();
            DeleteAllFiles();
            DeleteAllLinks();
        }

        private void SetupClient()
        {
            Client = AeroFSClient.Create(new AeroFSClient.Configuration
            {
                // read these values from app.config
                EndPoint = (string)Settings.Default["EndPoint"],
                AccessToken = (string)Settings.Default["AccessToken"],
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
