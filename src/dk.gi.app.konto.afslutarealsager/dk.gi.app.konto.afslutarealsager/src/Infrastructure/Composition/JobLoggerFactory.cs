using dk.gi.app.konto.afslutarealsager.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Composition
{
    public static class JobLoggerFactory
    {
        public static IJobLogger Create(KontoAfslutArealSagerSettings settings)
        {
            return Gi.Batch.Shared.Logging.JobLoggerFactory.Create(
                settings != null && settings.EnableLocalDebugLogging,
                settings?.LocalDebugLogPath,
                "dk.gi.app.konto.afslutarealsager");
        }
    }
}
