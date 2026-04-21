using dk.gi.app.contact.selskab.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.selskab.Infrastructure.Composition
{
    public static class JobLoggerFactory
    {
        public static IJobLogger Create(ContactSelskabSettings settings)
        {
            return Gi.Batch.Shared.Logging.JobLoggerFactory.Create(
                settings != null && settings.EnableLocalDebugLogging,
                settings?.LocalDebugLogPath,
                "dk.gi.app.contact.selskab");
        }
    }
}
