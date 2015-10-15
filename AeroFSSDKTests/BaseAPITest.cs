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
        public void SetupClient()
        {
            Client = AeroFSClient.Create(new AeroFSClient.Configuration
            {
                // read these values from app.config
                EndPoint = (string)Settings.Default["EndPoint"],
                AccessToken = (string)Settings.Default["AccessToken"],
            });

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

        protected void ExpectError(HttpStatusCode expected, Action action)
        {
            try
            {
                action.Invoke();
                Assert.Fail();
            }
            catch (WebException e)
            {
                Assert.IsInstanceOfType(e.Response, typeof(HttpWebResponse));
                Assert.AreEqual(expected, (e.Response as HttpWebResponse).StatusCode);
            }
        }
    }
}
