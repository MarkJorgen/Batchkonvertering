namespace dk.gi.app.konto.afslutarealsager.Application.Models
{
    public sealed class KontoAfslutArealSagerRequest
    {
        public string BrugerArealSager { get; }
        public int OpfoelgesFraPlusDage { get; }
        public string Mode { get; }
        public bool RunMode { get; }
        public string ForceIncidentId { get; }
        public string ForceSagsnummer { get; }
        public string ForceKontonr { get; }

        public KontoAfslutArealSagerRequest(
            string brugerArealSager,
            int opfoelgesFraPlusDage,
            string mode,
            bool runMode,
            string forceIncidentId,
            string forceSagsnummer,
            string forceKontonr)
        {
            BrugerArealSager = brugerArealSager ?? string.Empty;
            OpfoelgesFraPlusDage = opfoelgesFraPlusDage;
            Mode = mode ?? string.Empty;
            RunMode = runMode;
            ForceIncidentId = forceIncidentId ?? string.Empty;
            ForceSagsnummer = forceSagsnummer ?? string.Empty;
            ForceKontonr = forceKontonr ?? string.Empty;
        }

        public bool HasForcedCaseSelector => string.IsNullOrWhiteSpace(ForceIncidentId) == false || string.IsNullOrWhiteSpace(ForceSagsnummer) == false;
        public bool HasForcedAccountSelector => string.IsNullOrWhiteSpace(ForceKontonr) == false;
    }
}
