namespace dk.gi.app.contact.selskab.Application.Models
{
    public sealed class ResolvedServiceBusSettings
    {
        public string BaseUrl { get; }
        public string SasKeyName { get; }
        public string SasKey { get; }
        public string Source { get; }

        public bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(BaseUrl)
                    && !string.IsNullOrWhiteSpace(SasKeyName)
                    && !string.IsNullOrWhiteSpace(SasKey);
            }
        }

        public ResolvedServiceBusSettings(string baseUrl, string sasKeyName, string sasKey, string source)
        {
            BaseUrl = baseUrl ?? string.Empty;
            SasKeyName = sasKeyName ?? string.Empty;
            SasKey = sasKey ?? string.Empty;
            Source = source ?? string.Empty;
        }

        public static ResolvedServiceBusSettings Empty(string source)
        {
            return new ResolvedServiceBusSettings(string.Empty, string.Empty, string.Empty, source);
        }
    }
}
