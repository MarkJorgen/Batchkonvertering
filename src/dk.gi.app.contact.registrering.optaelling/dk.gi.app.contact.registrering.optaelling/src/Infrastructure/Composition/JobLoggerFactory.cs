using dk.gi.app.contact.registrering.optaelling.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.registrering.optaelling.Infrastructure.Composition
{
    internal static class JobLoggerFactory
    {
        public static IJobLogger Create(ContactRegistreringOptaellingSettings settings)
        {
            return Gi.Batch.Shared.Logging.JobLoggerFactory.Create(
                settings != null && settings.EnableLocalDebugLogging,
                settings?.LocalDebugLogPath,
                "dk.gi.app.contact.registrering.optaelling");
        }
    }
}
