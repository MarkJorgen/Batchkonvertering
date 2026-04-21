using dk.gi.app.contact.lassox.ophoer.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.contact.lassox.ophoer.Infrastructure.Config
{
    public static class LassoXOphoerConfigurationFactory
    {
        public static JobConfiguration CreateRaw(string[] args)
        {
            var loader = new JobConfigurationLoader(new AzureAppConfigurationSettingsSource());
            return loader.Load(args);
        }

        public static LassoXOphoerSettings CreateSettings(JobConfiguration configuration)
        {
            return LassoXOphoerSettings.Create(configuration);
        }
    }
}
