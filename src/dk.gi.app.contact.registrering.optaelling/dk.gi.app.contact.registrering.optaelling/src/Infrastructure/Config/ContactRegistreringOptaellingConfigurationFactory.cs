using dk.gi.app.contact.registrering.optaelling.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.contact.registrering.optaelling.Infrastructure.Config
{
    public static class ContactRegistreringOptaellingConfigurationFactory
    {
        public static JobConfiguration CreateRaw(string[] args)
        {
            var loader = new JobConfigurationLoader(new AzureAppConfigurationSettingsSource());
            return loader.Load(args);
        }

        public static ContactRegistreringOptaellingSettings CreateSettings(JobConfiguration configuration)
        {
            return ContactRegistreringOptaellingSettings.Create(configuration);
        }
    }
}
