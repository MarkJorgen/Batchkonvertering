using dk.gi.app.contact.registrering.optaelling.Application.Models;

namespace dk.gi.app.contact.registrering.optaelling.Infrastructure.Crm
{
    internal static class ContactRegistreringOperationResult
    {
        public static ContactRegistreringExecutionSummary Success(string message, string source)
        {
            return new ContactRegistreringExecutionSummary(
                success: true,
                closedExpiredTreklipOwnerRegistrations: false,
                createdJobsForContacts: false,
                message: message,
                source: source);
        }

        public static ContactRegistreringExecutionSummary Failure(string message, string source)
        {
            return new ContactRegistreringExecutionSummary(
                success: false,
                closedExpiredTreklipOwnerRegistrations: false,
                createdJobsForContacts: false,
                message: message,
                source: source);
        }
    }
}
