using System;
using System.IO;
using System.Text;

namespace Gi.Batch.Shared.Logging
{
    public sealed class FileJobLogger : IJobLogger
    {
        private static readonly object SyncRoot = new object();
        private readonly string _filePath;

        public FileJobLogger(string directoryPath, string applicationName)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException("A log directory path is required.", nameof(directoryPath));
            }

            if (string.IsNullOrWhiteSpace(applicationName))
            {
                throw new ArgumentException("An application name is required.", nameof(applicationName));
            }

            Directory.CreateDirectory(directoryPath);
            _filePath = Path.Combine(directoryPath, applicationName + ".log");
        }

        public void Info(string message) => Write("INFO", message, null);

        public void Warning(string message) => Write("WARN", message, null);

        public void Error(string message, Exception exception = null) => Write("ERROR", message, exception);

        private void Write(string level, string message, Exception exception)
        {
            var builder = new StringBuilder();
            builder.Append('[').Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("] ");
            builder.Append('[').Append(level).Append("] ");
            builder.AppendLine(message ?? string.Empty);

            if (exception != null)
            {
                builder.AppendLine(exception.ToString());
            }

            lock (SyncRoot)
            {
                File.AppendAllText(_filePath, builder.ToString(), Encoding.UTF8);
            }
        }
    }
}
