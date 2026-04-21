using dk.gi.app.contact.selskab.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.contact.selskab.Infrastructure.Config
{
    public static class ContactSelskabConfigurationFactory
    {
        public static JobConfiguration CreateRaw(string[] args)
        {
            var loader = new JobConfigurationLoader(new AzureAppConfigurationSettingsSource());
            return loader.Load(args);
        }

        public static ContactSelskabSettings CreateSettings(JobConfiguration configuration)
        {
            return ContactSelskabSettings.Create(configuration);
        }
    }
}
