using System;

namespace Gi.Batch.Shared.Logging
{
    public sealed class CompositeJobLogger : IJobLogger
    {
        private readonly IJobLogger[] _loggers;

        public CompositeJobLogger(params IJobLogger[] loggers)
        {
            _loggers = loggers ?? Array.Empty<IJobLogger>();
        }

        public void Info(string message)
        {
            foreach (var logger in _loggers)
            {
                logger?.Info(message);
            }
        }

        public void Warning(string message)
        {
            foreach (var logger in _loggers)
            {
                logger?.Warning(message);
            }
        }

        public void Error(string message, Exception exception = null)
        {
            foreach (var logger in _loggers)
            {
                logger?.Error(message, exception);
            }
        }
    }
}
