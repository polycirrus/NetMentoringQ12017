using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ScannerService
{
    public static class FileHelper
    {
        private static readonly int RetryAttempts = 3;
        private static readonly int SleepInterval = 1000;

        public static void Delete(string path)
        {
            Retry(() => File.Delete(path));
        }

        public static void Delete(IEnumerable<string> paths)
        {
            foreach (var path in paths)
                Delete(path);
        }

        public static void Retry(Action action)
        {
            Retry(action, RetryAttempts);
        }

        public static void Retry(Action action, int attempts)
        {
            var attemptCount = 0;
            while (attemptCount < attempts)
            {
                attemptCount++;
                try
                {
                    action();
                    break;
                }
                catch (IOException)
                {
                    if (attemptCount >= attempts)
                        throw;
                }
                catch (UnauthorizedAccessException)
                {
                    if (attemptCount >= attempts)
                        throw;
                }

                Thread.Sleep(SleepInterval);
            }
        }
    }
}