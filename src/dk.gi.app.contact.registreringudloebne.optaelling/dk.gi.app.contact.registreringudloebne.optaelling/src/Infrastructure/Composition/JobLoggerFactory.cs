using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Composition
{
    internal static class JobLoggerFactory
    {
        public static IJobLogger Create(ContactRegistreringUdloebneOptaellingSettings settings)
        {
            return Gi.Batch.Shared.Logging.JobLoggerFactory.Create(
                settings != null && settings.EnableLocalDebugLogging,
                settings?.LocalDebugLogPath,
                "dk.gi.app.contact.registreringudloebne.optaelling");
        }
    }
}
