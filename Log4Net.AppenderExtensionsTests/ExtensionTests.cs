using System;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using log4net;
using log4net.Config;
using log4net.Extensions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Log4Net.ExtensionsTests.MockUp;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace Log4Net.ExtensionsTests.MockUp
{
    [TestClass()]
    public class ExtensionTests
    {
        [TestMethod()]
        public void RemoveOldLogFilesTest()
        {
            ILog Log = LogManager.GetLogger(typeof(ExtensionTests));

            Log.Info("starting ...");

            var log4netConfig = new FileInfo("log4net.config");
            Assert.IsTrue(log4netConfig.Exists);

            _ = XmlConfigurator.Configure(log4netConfig);

            var logRepo = LogManager.GetRepository();
            var appenderList = ((Hierarchy)logRepo).Root.Appenders.OfType<RollingFileAppender>();
            int countRemoved = 0;
            foreach (var appender in appenderList)
            {
                var count = BackLogDays - appender.MaxSizeRollBackups;
                countRemoved += count < 0 ? 0 : count;
            }

            IList<string> removedList;
            removedList = LogManager.GetRepository().RemoveOldLogFiles();
            
            Assert.IsTrue(removedList.Any());
            Assert.IsTrue(removedList.Count == countRemoved);
            
            Log.Info("... closing");
            LogManager.Shutdown();
        }

        public static int BackLogDays => 5;

        [TestInitialize]
        public void InitializeTest()
        {
            LogFilesMock.CreateSomeLogFiles(@"Logs\LogFile.log", "_yyyyMMdd", BackLogDays);
            LogFilesMock.CreateSomeLogFiles(@"Logs\LogFile2.log", "_yyyyMMdd", BackLogDays);
        }
    }
}