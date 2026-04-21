namespace Gi.Batch.Shared.Logging
{
    public static class JobLoggerFactory
    {
        public static IJobLogger Create(bool enableLocalDebugLogging, string localDebugLogPath, string applicationName)
        {
            if (enableLocalDebugLogging)
            {
                return new CompositeJobLogger(
                    new ConsoleJobLogger(),
                    new FileJobLogger(localDebugLogPath, applicationName));
            }

            return new ConsoleJobLogger();
        }
    }
}
