using System;

namespace Gi.Batch.Shared.Logging
{
    public sealed class ConsoleJobLogger : IJobLogger
    {
        public void Info(string message) => Write("INFO", message);

        public void Warning(string message) => Write("WARN", message);

        public void Error(string message, Exception exception = null)
        {
            if (exception == null)
            {
                Write("ERROR", message);
                return;
            }

            Write("ERROR", message + Environment.NewLine + exception);
        }

        private static void Write(string level, string message)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
        }
    }
}
