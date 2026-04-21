using dk.gi.app.konto.kontoejerLuk.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.kontoejerLuk.Infrastructure.Composition
{
    public static class JobLoggerFactory
    {
        public static IJobLogger Create(KontoejerLukSettings settings)
        {
            return Gi.Batch.Shared.Logging.JobLoggerFactory.Create(
                settings != null && settings.EnableLocalDebugLogging,
                settings?.LocalDebugLogPath,
                "dk.gi.app.konto.kontoejerLuk");
        }
    }
}
