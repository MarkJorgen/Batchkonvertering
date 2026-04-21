using dk.gi.app.konto.startarealtjek.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.startarealtjek.Infrastructure.Composition
{
    public static class JobLoggerFactory
    {
        public static IJobLogger Create(KontoStartArealTjekSettings settings)
        {
            return Gi.Batch.Shared.Logging.JobLoggerFactory.Create(
                settings != null && settings.EnableLocalDebugLogging,
                settings?.LocalDebugLogPath,
                "dk.gi.app.konto.startarealtjek");
        }
    }
}
