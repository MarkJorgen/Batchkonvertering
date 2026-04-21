namespace dk.gi.app.contact.lassox.ophoer.Application.Models
{
    public sealed class LassoXOphoerExecutionSummary
    {
        public bool Success { get; }
        public int ScannedContacts { get; }
        public int ContactsMarkedForUnsubscribe { get; }
        public int UpdatedContacts { get; }
        public string Message { get; }
        public string Source { get; }

        public LassoXOphoerExecutionSummary(
            bool success,
            int scannedContacts,
            int contactsMarkedForUnsubscribe,
            int updatedContacts,
            string message,
            string source)
        {
            Success = success;
            ScannedContacts = scannedContacts;
            ContactsMarkedForUnsubscribe = contactsMarkedForUnsubscribe;
            UpdatedContacts = updatedContacts;
            Message = message ?? string.Empty;
            Source = source ?? string.Empty;
        }
    }
}
