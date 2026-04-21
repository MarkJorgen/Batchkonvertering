namespace dk.gi.app.contact.selskab.Application.Models
{
    public sealed class ContactSelskabExecutionSummary
    {
        public bool Success { get; }
        public int ScannedOwnerRows { get; }
        public int QualifiedCompanies { get; }
        public int PublishedJobs { get; }
        public string Message { get; }
        public string Source { get; }

        public ContactSelskabExecutionSummary(
            bool success,
            int scannedOwnerRows,
            int qualifiedCompanies,
            int publishedJobs,
            string message,
            string source)
        {
            Success = success;
            ScannedOwnerRows = scannedOwnerRows;
            QualifiedCompanies = qualifiedCompanies;
            PublishedJobs = publishedJobs;
            Message = message ?? string.Empty;
            Source = source ?? string.Empty;
        }
    }
}
