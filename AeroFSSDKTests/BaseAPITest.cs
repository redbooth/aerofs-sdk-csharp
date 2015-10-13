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
    /// <summary>
    /// This is an end-to-end testsuite designed to work with an actual API endpoint.
    /// </summary>
    /// <remarks>
    /// To run these tests, one needs to:
    /// - run an actual API endpoint.
    /// - obtain an actual access token.
    /// - note that the test will delete any existing files prior to running tests, so
    ///     it is best to create a test user account for testing purpose instead of
    ///     using a personal account and risk having one's files deleted.
    /// - go to Project -> Properties... -> Settings and edit EndPoint and AccessToken
    ///     to reflect the values to use.
    /// </remarks>
    [TestClass]
    public class BaseAPITest : BaseTest
    {
        protected AeroFSAPI Client { get; set; }

        [TestInitialize]
        public void SetupClient()
        {
            Client = AeroFSClient.Create(new AeroFSClient.Configuration
            {
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
