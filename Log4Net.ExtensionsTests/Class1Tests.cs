using System;

using System.Collections;
using System.IO;
using System.Linq;

using log4net;
using log4net.Config;
using log4net.Extensions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Log4Net_Standalone.Tests
{
    [TestClass()]
    public class ExtensionTests
    {
        [TestMethod()]
        public void RemoveOldLogFilesTest()
        {
            ILog Log = LogManager.GetLogger(typeof(ExtensionTests));

            Log.Info("starting ...");

            ICollection logConfigCollection = XmlConfigurator.Configure(new FileInfo("log4net.config"));

            LogManager.GetRepository().RemoveOldLogFiles();
            Log.Info("running");

            LogManager.Shutdown();
        }
    }
}