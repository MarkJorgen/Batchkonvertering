using System;

namespace dk.gi.app.ejendom.tjekejerskifte.Application.Models
{
    public sealed class EjendomTjekEjerskifteCandidate
    {
        public Guid PropertyId { get; }
        public string KommuneNummer { get; }
        public string EjendomsNummer { get; }
        public string BbrNummer { get; }
        public string BfeNummer { get; }
        public string BfeNummerModerEjendom { get; }
        public DateTime? SidsteSkoedeDatoUtc { get; }

        public EjendomTjekEjerskifteCandidate(Guid propertyId, string kommuneNummer, string ejendomsNummer, string bbrNummer, string bfeNummer, string bfeNummerModerEjendom, DateTime? sidsteSkoedeDatoUtc)
        {
            PropertyId = propertyId;
            KommuneNummer = kommuneNummer ?? string.Empty;
            EjendomsNummer = ejendomsNummer ?? string.Empty;
            BbrNummer = bbrNummer ?? string.Empty;
            BfeNummer = bfeNummer ?? string.Empty;
            BfeNummerModerEjendom = bfeNummerModerEjendom ?? string.Empty;
            SidsteSkoedeDatoUtc = sidsteSkoedeDatoUtc;
        }
    }
}
