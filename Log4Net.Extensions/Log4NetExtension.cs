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

        static string GetSearchPattern(string fullFileName, string datePattern)
        {
            var suffix = Path.GetExtension(fullFileName);
            var baseFileName = Path.GetFileNameWithoutExtension(fullFileName);
            var lastIndex = baseFileName.Length - datePattern.Length;
            baseFileName = baseFileName.Substring(0, lastIndex);

            var searchPattern = baseFileName + "*" + suffix;

            return searchPattern;
        }

        static IList<string> RemoveOldLogFiles(string logDir, string searchPattern, int maxSizeRollBackups)
        {
            var oldest = DateTime.Today.AddDays(-maxSizeRollBackups);
            var matches = new DirectoryInfo(logDir)
                                .EnumerateFiles(searchPattern)
                                .Where(f => oldest <= f.CreationTime)
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

        public static IList<string> RemoveOldLogFiles(this ILoggerRepository logRepo, ILog log = null)
        {
            Log = log;
            try
            {
                if (logRepo == null) throw new NullReferenceException("no repositories found/configured");
                if (!logRepo.Configured) throw new InvalidOperationException("repository must be configured first");

                foreach (RollingFileAppender appender in ((Hierarchy)logRepo).Root.Appenders.OfType<RollingFileAppender>())
                {
                    if (appender == null) continue;
                    if (string.IsNullOrEmpty(appender.File)) continue;
                    if (appender.PreserveLogFileNameExtension != true) continue;
                    if (appender.MaxSizeRollBackups <= 0 || short.MaxValue <= appender.MaxSizeRollBackups) continue;

                    string appenderFile = appender.File;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    string logDir = Path.GetDirectoryName(appenderFile);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                    string searchPattern = GetSearchPattern(appender.File, appender.DatePattern);
#pragma warning disable CS8604 // Possible null reference argument.
                    IList<string> removedLogs = RemoveOldLogFiles(logDir, searchPattern, appender.MaxSizeRollBackups);
                    return removedLogs;
#pragma warning restore CS8604 // Possible null reference argument.
                }
                return null;
            }
            catch (Exception ex)
            {
                Log?.Error(ex.GetBaseException().Message, ex);
                return Array.Empty<string>();
            }
        }
    }
}
