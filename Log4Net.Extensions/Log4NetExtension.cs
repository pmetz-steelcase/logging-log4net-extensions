using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using log4net.Appender;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace log4net.Extensions
{
    public static class Log4NetExtension
    {
        static ILog Log { get; set; }

        static bool CanDeleteFiles(RollingFileAppender appender)
        {
            if (appender == null)
            {
                Log?.Error("appender is null", new NullReferenceException(nameof(appender)));
                return false;
            }
            if (string.IsNullOrEmpty(appender.File))
            {
                Log?.Error($"appender {appender.Name} has no file value", new ArgumentNullException());
                return false;
            }
            if (appender.PreserveLogFileNameExtension != true)
            {
                Log?.Error($"appender {appender.Name} must not equal true for PreserveLogFileNameExtension",
                           new NotImplementedException());
                return false;
            }
            if (appender.MaxSizeRollBackups < 0 || short.MaxValue <= appender.MaxSizeRollBackups)
            {
                Log?.Error($"appender {appender.Name} MaxSizeRollBackups value is invalid",
                           new ArgumentOutOfRangeException(nameof(appender.MaxSizeRollBackups)));
                return false;
            }
            return true;
        }

        static (string pattern, int length) GetSearchPattern(string fullFileName, string datePattern)
        {
            var suffix = Path.GetExtension(fullFileName);
            var len = Path.GetFileName(fullFileName).Length;
            var baseFileName = Path.GetFileNameWithoutExtension(fullFileName);

            var lastIndex = baseFileName.Length - datePattern.Length;
            baseFileName = baseFileName.Substring(0, lastIndex);

            var searchPattern = baseFileName + "*" + suffix;

            return (searchPattern, len);
        }

        static IList<string> RemoveOldLogFiles(string logDir, string searchPattern, int fileNameLen, int maxSizeRollBackups)
        {
            var youngest = DateTime.Today.AddDays(-maxSizeRollBackups);
            var matches = new DirectoryInfo(logDir)
                                .EnumerateFiles(searchPattern)
                                .Where(f => f.Name.Length == fileNameLen)
                                .Where(f => f.CreationTime <= youngest)
                                .Where(f => !f.IsReadOnly);

            var deleted = new List<string>();
            foreach (FileInfo match in matches.ToList())
            {
                try
                {
                    match.Delete();
                    deleted.Add(match.Name);
                }
                catch (Exception ex)
                {
                    Log?.Error("can't delete old log file: \n" +
                              match.FullName + "\n" +
                              ex.GetBaseException().Message);
                }

            }
            return deleted;
        }

        /// <summary>
        /// takes value of MaxSizeRollBackups for matching outraged files
        /// value == age days
        /// the date pattern must be a fixed length pattern, f.eg. "_yyyyMMdd"
        /// </summary>
        /// <param name="logRepo"></param>
        /// <param name="log"></param>
        /// <returns>list of deleted files</returns>
        public static IList<string> RemoveOldLogFiles(this ILoggerRepository logRepo, ILog log = null)
        {
            Log = log;
            IList<string> removedLogs = new List<string>();
            try
            {
                if (logRepo == null) throw new NullReferenceException("no repositories found/configured");
                //if (!logRepo.Configured) throw new InvalidOperationException("repository must be configured first");
                var rollingFileAppenderList = ((Hierarchy)logRepo).Root.Appenders.OfType<RollingFileAppender>();
                foreach (RollingFileAppender appender in rollingFileAppenderList)
                {
                    if (!CanDeleteFiles(appender)) continue;

                    string appenderFile = appender.File;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    string logDir = Path.GetDirectoryName(appenderFile);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                    (string searchPattern, int length) = GetSearchPattern(appender.File, appender.DatePattern);
#pragma warning disable CS8604 // Possible null reference argument.
                    var list = RemoveOldLogFiles(logDir, searchPattern, length, appender.MaxSizeRollBackups);
                    foreach (var item in list)
                        removedLogs.Add(item);
#pragma warning restore CS8604 // Possible null reference argument.
                }
            }
            catch (Exception ex)
            {
                Log?.Error(ex.GetBaseException().Message, ex);
            }
            return removedLogs;
        }
    }
}
