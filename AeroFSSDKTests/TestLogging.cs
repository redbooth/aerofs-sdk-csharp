using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class TestLogging : BaseTest
    {
        [TestMethod]
        public void TestLoggingConfiguration()
        {
            // inspect the output for this test case to observe the effects
            // and confirm whether the logger is working correctly.
            Logger.Info("Hello World!");
        }
    }
}
