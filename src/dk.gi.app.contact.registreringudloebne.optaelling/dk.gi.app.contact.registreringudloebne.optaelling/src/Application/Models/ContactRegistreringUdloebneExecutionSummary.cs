namespace dk.gi.app.contact.registreringudloebne.optaelling.Application.Models
{
    public sealed class ContactRegistreringUdloebneExecutionSummary
    {
        public bool Success { get; }
        public int ScannedRegistreringer { get; }
        public int QueuedRegistreringJobs { get; }
        public string Message { get; }
        public string Source { get; }

        public ContactRegistreringUdloebneExecutionSummary(
            bool success,
            int scannedRegistreringer,
            int queuedRegistreringJobs,
            string message,
            string source)
        {
            Success = success;
            ScannedRegistreringer = scannedRegistreringer;
            QueuedRegistreringJobs = queuedRegistreringJobs;
            Message = message ?? string.Empty;
            Source = source ?? string.Empty;
        }
    }
}
