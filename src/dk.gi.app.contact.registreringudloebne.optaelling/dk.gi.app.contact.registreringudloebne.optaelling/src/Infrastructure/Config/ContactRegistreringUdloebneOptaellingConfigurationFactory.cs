using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Config
{
    public static class ContactRegistreringUdloebneOptaellingConfigurationFactory
    {
        public static JobConfiguration CreateRaw(string[] args)
        {
            var loader = new JobConfigurationLoader(new AzureAppConfigurationSettingsSource());
            return loader.Load(args);
        }

        public static ContactRegistreringUdloebneOptaellingSettings CreateSettings(JobConfiguration configuration)
        {
            return ContactRegistreringUdloebneOptaellingSettings.Create(configuration);
        }
    }
}
