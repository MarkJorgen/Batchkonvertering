namespace dk.gi.app.ejendom.tjekejerskifte.Application.Models
{
    public sealed class EjendomTjekEjerskifteExecutionSummary
    {
        public bool Success { get; }
        public int ScannedProperties { get; }
        public int PublishedJobs { get; }
        public int SkippedWithoutBfe { get; }
        public string Message { get; }
        public string Source { get; }

        public EjendomTjekEjerskifteExecutionSummary(bool success, int scannedProperties, int publishedJobs, int skippedWithoutBfe, string message, string source)
        {
            Success = success;
            ScannedProperties = scannedProperties;
            PublishedJobs = publishedJobs;
            SkippedWithoutBfe = skippedWithoutBfe;
            Message = message ?? string.Empty;
            Source = source ?? string.Empty;
        }
    }
}
