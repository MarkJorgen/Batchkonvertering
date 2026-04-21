using dk.gi.app.konto.afslutarealsager.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Config
{
    public static class KontoAfslutArealSagerRequestFactory
    {
        public static KontoAfslutArealSagerRequest Create(JobConfiguration configuration, KontoAfslutArealSagerSettings settings)
        {
            string bruger = configuration.Get("BrugerArealSager", settings.BrugerArealSager);
            int opfoelgesFraPlusDage = configuration.GetInt("OpfoelgesFraPlusDage", settings.OpfoelgesFraPlusDage);
            string forceIncidentId = configuration.Get("ForceIncidentId", settings.ForceIncidentId);
            string forceSagsnummer = configuration.Get("ForceSagsnummer", settings.ForceSagsnummer);
            string forceKontonr = configuration.Get("ForceKontonr", settings.ForceKontonr);
            return new KontoAfslutArealSagerRequest(bruger, opfoelgesFraPlusDage, settings.Mode, settings.RunMode, forceIncidentId, forceSagsnummer, forceKontonr);
        }
    }
}
