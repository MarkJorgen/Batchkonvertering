namespace dk.gi.app.konto.startarealtjek.Application.Models
{
    public sealed class KontoStartArealTjekPublishResult
    {
        public bool Success { get; }
        public int PublishedCount { get; }
        public string Message { get; }

        public KontoStartArealTjekPublishResult(bool success, int publishedCount, string message)
        {
            Success = success;
            PublishedCount = publishedCount;
            Message = message ?? string.Empty;
        }
    }
}
