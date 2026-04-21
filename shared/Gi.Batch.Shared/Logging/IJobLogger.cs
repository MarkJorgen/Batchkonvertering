using System;

namespace Gi.Batch.Shared.Logging
{
    public interface IJobLogger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception exception = null);
    }
}
