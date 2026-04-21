namespace dk.gi.app.konto.startarealtjek.Application.Models
{
    public sealed class KontoStartArealTjekRequest
    {
        public string KontoNr { get; }
        public string Mode { get; }
        public bool RunMode { get; }

        public KontoStartArealTjekRequest(string kontoNr, string mode, bool runMode)
        {
            KontoNr = kontoNr ?? string.Empty;
            Mode = mode ?? string.Empty;
            RunMode = runMode;
        }
    }
}
