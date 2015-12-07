using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestLinkSharing : BaseAPITest
    {
        private FileID FileID { get; set; }
        private ShareID ShareID { get; set; }
        private LinkID Key { get; set; }

        [TestInitialize]
        public void CreateLink()
        {
            FileID = Client.CreateFile(FolderID.Root, "TestLinkSharing").ID;
            ShareID = FileID.ShareID;
            Key = Client.CreateLink(FileID).Key;

            Client.RemoveLinkPassword(ShareID, Key);
            Client.UpdateLinkRequireLogin(ShareID, Key, false);
            Client.RemoveLinkExpiry(ShareID, Key);

            var link = GetLinkInfo();
            Assert.IsFalse(link.HasPassword);
            Assert.IsFalse(link.RequireLogin);
            Assert.AreEqual(0, link.Expires);
        }

        [TestMethod]
        public void ShouldDeleteLinks()
        {
            Client.DeleteLink(ShareID, Key);
            ExpectError(HttpStatusCode.NotFound, () => GetLinkInfo());
        }

        [TestMethod]
        public void ShouldCreateLinksWithPasswords()
        {
            var key = Client.CreateLink(FileID, password: "fake_password").Key;
            var link = Client.GetLinkInfo(ShareID, key);

            Assert.IsTrue(link.HasPassword);
        }

        [TestMethod]
        public void ShouldCreateLinksWithExpiry()
        {
            var key = Client.CreateLink(FileID, expiry: 5000L).Key;
            var link = Client.GetLinkInfo(ShareID, key);

            // note that this will accidentally fail if the current server time happens
            // to be 5000L UTF which shouldn't be the case because it's 2015.
            Assert.AreNotEqual(0L, link.Expires);
        }

        [TestMethod]
        public void ShouldCreateLinksToRequireLogin()
        {
            foreach (var requireLogin in new[] { false, true })
            {
                var key = Client.CreateLink(FileID, requireLogin: requireLogin).Key;
                var link = Client.GetLinkInfo(ShareID, key);

                Assert.AreEqual(requireLogin, link.RequireLogin);
            }
        }

        [TestMethod]
        public void ShouldCreateLinksWithMultipleProperties()
        {
            var key = Client.CreateLink(FileID,
                password: "new_password", requireLogin: true, expiry: 5000L).Key;
            var link = Client.GetLinkInfo(ShareID, key);

            Assert.IsTrue(link.HasPassword);
            Assert.IsTrue(link.RequireLogin);
            Assert.AreNotEqual(0, link.Expires);
        }

        [TestMethod]
        public void ShouldListLinks()
        {
            var keys = new[]
            {
                Key,
                Client.CreateLink(FileID).Key,
            }.ToSet();

            var links = Client.ListLinks(ShareID);

            Assert.IsTrue(keys.SetEquals(links.Select(link => link.Key)));
        }

        [TestMethod]
        public void ShouldUpdateLinkPassword()
        {
            var mutators = new Func<string, Link>[]
            {
                password => Client.UpdateLinkInfo(ShareID, Key, password: password),
                password => Client.UpdateLinkPassword(ShareID, Key, password),
            };

            foreach (var mutator in mutators)
            {
                mutator("new_password");
                Assert.IsTrue(GetLinkInfo().HasPassword);

                mutator("next_password");
                Assert.IsTrue(GetLinkInfo().HasPassword);

                Client.RemoveLinkPassword(ShareID, Key);
                Assert.IsFalse(GetLinkInfo().HasPassword);

                Client.RemoveLinkPassword(ShareID, Key);
                Assert.IsFalse(GetLinkInfo().HasPassword);
            }
        }

        [TestMethod]
        public void ShouldUpdateLinkRequireLogin()
        {
            var mutators = new Func<bool, Link>[]
            {
                state => Client.UpdateLinkInfo(ShareID, Key, requireLogin: state),
                state => Client.UpdateLinkRequireLogin(ShareID, Key, state),
            };
            // N.B. cover each state transition
            var states = new[] { true, true, false, false };

            foreach (var mutator in mutators)
            {
                foreach (var state in states)
                {
                    mutator(state);
                    Assert.AreEqual(state, Client.GetLinkInfo(ShareID, Key).RequireLogin);
                }
            }
        }

        // since we cannot control server time, we'll have to be a little more creative
        // to verify that expiry works.
        [TestMethod]
        public void ShouldUpdateExpiry()
        {
            var mutators = new Func<long, Link>[]
            {
                expires => Client.UpdateLinkInfo(ShareID, Key, expiry: expires),
                expires => Client.UpdateLinkExpiry(ShareID, Key, expires),
            };

            foreach (var mutator in mutators)
            {
                mutator(10000000L);
                var oldExpires = GetLinkInfo().Expires;
                Assert.AreNotEqual(0, oldExpires);

                mutator(5000L);
                var newExpires = GetLinkInfo().Expires;
                Assert.AreNotEqual(0, newExpires);
                // N.B. this only works because oldExpires >> newExpires
                Assert.IsTrue(oldExpires > newExpires + 1000L - 5L);

                Client.RemoveLinkExpiry(ShareID, Key);
                Assert.AreEqual(0, GetLinkInfo().Expires);
                Client.RemoveLinkExpiry(ShareID, Key);
                Assert.AreEqual(0, GetLinkInfo().Expires);
            }
        }

        [TestMethod]
        public void ShouldUpdateMultiplePropertiesAtOnce()
        {
            Client.UpdateLinkInfo(ShareID, Key,
                password: "new_password", requireLogin: true, expiry: 5000L);

            var link = GetLinkInfo();
            Assert.IsTrue(link.HasPassword);
            Assert.IsTrue(link.RequireLogin);
            Assert.AreNotEqual(0, link.Expires);
        }

        [TestMethod]
        public void ShouldNotUpdateUnrelatedLinkProperties()
        {
            Client.UpdateLinkInfo(ShareID, Key, password: "new_password");
            Assert.IsFalse(GetLinkInfo().RequireLogin);
            Assert.AreEqual(0, GetLinkInfo().Expires);
            Client.RemoveLinkPassword(ShareID, Key);

            Client.UpdateLinkInfo(ShareID, Key, requireLogin: true);
            Assert.IsFalse(GetLinkInfo().HasPassword);
            Assert.AreEqual(0, GetLinkInfo().Expires);

            Client.UpdateLinkInfo(ShareID, Key, expiry: 5000L);
            Assert.IsFalse(GetLinkInfo().HasPassword);
            Assert.IsTrue(GetLinkInfo().RequireLogin);
        }

        private Link GetLinkInfo()
        {
            return Client.GetLinkInfo(ShareID, Key);
        }
    }
}
