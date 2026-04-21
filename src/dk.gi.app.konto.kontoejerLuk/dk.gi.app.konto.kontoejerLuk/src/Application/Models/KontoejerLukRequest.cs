namespace dk.gi.app.konto.kontoejerLuk.Application.Models
{
    public sealed class KontoejerLukRequest
    {
        public string Mode { get; }
        public bool RunMode { get; }

        public KontoejerLukRequest(string mode, bool runMode)
        {
            Mode = mode ?? string.Empty;
            RunMode = runMode;
        }
    }
}
