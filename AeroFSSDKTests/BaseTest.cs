using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace AeroFSSDK.Tests
{
    [TestClass]
    public class BaseTest
    {
        // TODO: this pattern is wrong and doesn't work as well as we'd like
        const string DefaultPattern = "%-5level [%date{ISO8601, UTC}] [%-8.8thread] %c{1}: %m%n";

        protected ILog Logger { get; set; }

        [AssemblyInitialize]
        public static void SetupLogging(TestContext context)
        {
            var layout = new PatternLayout
            {
                ConversionPattern = DefaultPattern
            };
            layout.ActivateOptions();

            var appender = new ConsoleAppender
            {
                Target = ConsoleAppender.ConsoleOut,
                Layout = layout
            };
            appender.ActivateOptions();

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.Level = Level.Info;
            hierarchy.Root.AddAppender(appender);
            hierarchy.Configured = true;
        }

        [TestInitialize]
        public void SetupLogger()
        {
            Logger = LogManager.GetLogger(GetType());
        }

        protected void Info<T>(T obj)
        {
            var type = typeof(T);

            foreach (var prop in type.GetProperties())
            {
                Logger.Info(string.Format("{0}: {1}", prop.Name, prop.GetValue(obj, null)));
            }
        }
    }
}
