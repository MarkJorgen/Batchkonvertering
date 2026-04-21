using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Crm
{
    internal static class ContactRegistreringUdloebneOperationResult
    {
        public static ContactRegistreringUdloebneExecutionSummary Success(string message, string source)
            => new ContactRegistreringUdloebneExecutionSummary(true, 0, 0, message, source);

        public static ContactRegistreringUdloebneExecutionSummary Failure(string message, string source)
            => new ContactRegistreringUdloebneExecutionSummary(false, 0, 0, message, source);
    }
}
