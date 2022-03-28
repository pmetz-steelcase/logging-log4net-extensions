using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Log4Net.ExtensionsTests.MockUp
{
    public class LogFilesMock
    {
        static string GetDateInfix(string datePattern, int n)
        {
            string dateInThePast = DateTime.Today.AddDays(-n).ToString(datePattern);
            return dateInThePast;
        }

        public static void CreateSomeLogFiles(string fileFullName, string datePattern, int countDaysInThePast)
        {
            var suffix = Path.GetExtension(fileFullName);
            var name = Path.GetFileNameWithoutExtension(fileFullName);
            var dir = Path.GetDirectoryName(fileFullName);
            for (int n = countDaysInThePast; 0 <= n; n--)
            {
                string infix = GetDateInfix(datePattern, n);
                string fullName = Path.Combine(dir, $@"{name}{infix}{suffix}");

                var logFile = new FileInfo(fullName);

                var outDir = new DirectoryInfo(Path.GetDirectoryName(fullName));
                if (!outDir.Exists) outDir.Create();

                using StreamWriter writer = logFile.Exists ? logFile.AppendText() : logFile.CreateText();
                writer.WriteLine($@"{DateTime.Now:s} that's some text from mock up");
                writer.Close();
                //make file older
                logFile.CreationTime = DateTime.Now.AddDays(-n);
            }
        }
    }
}
