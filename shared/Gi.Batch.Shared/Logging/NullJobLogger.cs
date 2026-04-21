using System;

namespace Gi.Batch.Shared.Logging
{
    public sealed class NullJobLogger : IJobLogger
    {
        public void Info(string message) { }
        public void Warning(string message) { }
        public void Error(string message, Exception exception = null) { }
    }
}
