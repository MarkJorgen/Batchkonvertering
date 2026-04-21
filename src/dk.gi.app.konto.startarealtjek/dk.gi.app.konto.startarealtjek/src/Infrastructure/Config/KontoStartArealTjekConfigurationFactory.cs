using dk.gi.app.konto.startarealtjek.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.konto.startarealtjek.Infrastructure.Config
{
    public static class KontoStartArealTjekConfigurationFactory
    {
        public static JobConfiguration CreateRaw(string[] args)
        {
            var loader = new JobConfigurationLoader(new AzureAppConfigurationSettingsSource());
            return loader.Load(args);
        }

        public static KontoStartArealTjekSettings CreateSettings(JobConfiguration configuration)
            => KontoStartArealTjekSettings.Create(configuration);
    }
}
