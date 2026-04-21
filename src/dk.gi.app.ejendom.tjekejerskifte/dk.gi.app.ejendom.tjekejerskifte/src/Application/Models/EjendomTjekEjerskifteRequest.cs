namespace dk.gi.app.ejendom.tjekejerskifte.Application.Models
{
    public sealed class EjendomTjekEjerskifteRequest
    {
        public int MaxDage { get; }
        public int MaxAntal { get; }
        public string Mode { get; }
        public bool RunMode { get; }

        public EjendomTjekEjerskifteRequest(int maxDage, int maxAntal, string mode, bool runMode)
        {
            MaxDage = maxDage;
            MaxAntal = maxAntal;
            Mode = mode ?? string.Empty;
            RunMode = runMode;
        }
    }
}
