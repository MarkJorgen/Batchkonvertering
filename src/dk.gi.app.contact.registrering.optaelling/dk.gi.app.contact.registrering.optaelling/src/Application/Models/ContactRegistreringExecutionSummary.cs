namespace dk.gi.app.contact.registrering.optaelling.Application.Models
{
    public sealed class ContactRegistreringExecutionSummary
    {
        public bool Success { get; }
        public bool ClosedExpiredTreklipOwnerRegistrations { get; }
        public bool CreatedJobsForContacts { get; }
        public string Message { get; }
        public string Source { get; }

        public ContactRegistreringExecutionSummary(
            bool success,
            bool closedExpiredTreklipOwnerRegistrations,
            bool createdJobsForContacts,
            string message,
            string source)
        {
            Success = success;
            ClosedExpiredTreklipOwnerRegistrations = closedExpiredTreklipOwnerRegistrations;
            CreatedJobsForContacts = createdJobsForContacts;
            Message = message ?? string.Empty;
            Source = source ?? string.Empty;
        }
    }
}
