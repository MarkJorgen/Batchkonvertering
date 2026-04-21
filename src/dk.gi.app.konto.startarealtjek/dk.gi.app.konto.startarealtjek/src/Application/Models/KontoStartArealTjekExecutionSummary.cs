namespace dk.gi.app.konto.startarealtjek.Application.Models
{
    public sealed class KontoStartArealTjekExecutionSummary
    {
        public bool Success { get; }
        public int ScannedAccounts { get; }
        public int SubjectAccounts { get; }
        public int NonSubjectAccounts { get; }
        public int PublishedJobs { get; }
        public string Message { get; }
        public string Source { get; }

        public KontoStartArealTjekExecutionSummary(
            bool success,
            int scannedAccounts,
            int subjectAccounts,
            int nonSubjectAccounts,
            int publishedJobs,
            string message,
            string source)
        {
            Success = success;
            ScannedAccounts = scannedAccounts;
            SubjectAccounts = subjectAccounts;
            NonSubjectAccounts = nonSubjectAccounts;
            PublishedJobs = publishedJobs;
            Message = message ?? string.Empty;
            Source = source ?? string.Empty;
        }
    }
}
