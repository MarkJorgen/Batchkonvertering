namespace dk.gi.app.contact.selskab.Application.Models
{
    public sealed class ContactSelskabPublishResult
    {
        public bool Success { get; }
        public int PublishedCount { get; }
        public string Message { get; }

        public ContactSelskabPublishResult(bool success, int publishedCount, string message)
        {
            Success = success;
            PublishedCount = publishedCount;
            Message = message ?? string.Empty;
        }
    }
}
