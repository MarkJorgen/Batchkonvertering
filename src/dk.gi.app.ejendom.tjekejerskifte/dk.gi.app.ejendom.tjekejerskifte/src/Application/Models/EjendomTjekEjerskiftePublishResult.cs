namespace dk.gi.app.ejendom.tjekejerskifte.Application.Models
{
    public sealed class EjendomTjekEjerskiftePublishResult
    {
        public bool Success { get; }
        public int PublishedCount { get; }
        public int SkippedWithoutBfe { get; }
        public string Message { get; }
        public EjendomTjekEjerskiftePublishResult(bool success, int publishedCount, int skippedWithoutBfe, string message)
        { Success = success; PublishedCount = publishedCount; SkippedWithoutBfe = skippedWithoutBfe; Message = message ?? string.Empty; }
    }
}
