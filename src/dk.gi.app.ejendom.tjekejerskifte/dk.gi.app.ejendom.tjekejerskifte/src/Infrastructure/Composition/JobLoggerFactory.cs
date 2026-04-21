using dk.gi.app.ejendom.tjekejerskifte.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.ejendom.tjekejerskifte.Infrastructure.Composition
{
    public static class JobLoggerFactory
    {
        public static IJobLogger Create(EjendomTjekEjerskifteSettings settings)
        {
            return Gi.Batch.Shared.Logging.JobLoggerFactory.Create(
                settings != null && settings.EnableLocalDebugLogging,
                settings?.LocalDebugLogPath,
                "dk.gi.app.ejendom.tjekejerskifte");
        }
    }
}
