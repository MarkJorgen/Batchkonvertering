using dk.gi.app.contact.lassox.ophoer.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.lassox.ophoer.Infrastructure.Composition
{
    public static class JobLoggerFactory
    {
        public static IJobLogger Create(LassoXOphoerSettings settings)
        {
            return Gi.Batch.Shared.Logging.JobLoggerFactory.Create(
                settings != null && settings.EnableLocalDebugLogging,
                settings?.LocalDebugLogPath,
                "dk.gi.app.contact.lassox.ophoer");
        }
    }
}
