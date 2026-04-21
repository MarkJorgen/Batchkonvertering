namespace dk.gi.app.contact.registreringudloebne.optaelling.Application.Models
{
    public sealed class RunoutRegistreringPublishResult
    {
        public bool Success { get; }
        public int PublishedCount { get; }
        public int SkippedCount { get; }
        public string Message { get; }

        public RunoutRegistreringPublishResult(bool success, int publishedCount, int skippedCount, string message)
        {
            Success = success;
            PublishedCount = publishedCount;
            SkippedCount = skippedCount;
            Message = message ?? string.Empty;
        }
    }
}
