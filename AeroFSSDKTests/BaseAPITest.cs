using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using AeroFSSDK.Impl;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class BaseAPITest : BaseTest
    {
        protected AeroFSAPI Client { get; set; }
        protected AeroFSAPI OrgAdminClient { get; set; }

        [TestInitialize]
        public void SetupClients()
        {
            Client = AeroFSClient.Create(Settings.AccessToken, new AeroFSClient.Configuration
            {
                HostName = Settings.HostName,
                APIVersion = Settings.APIVersion
            });

            OrgAdminClient = AeroFSClient.Create(Settings.OrgAdminAccessToken, new AeroFSClient.Configuration
            {
                HostName = Settings.HostName,
                APIVersion = Settings.APIVersion
            });
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
