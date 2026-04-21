using dk.gi.app.konto.kontoejerLuk.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.konto.kontoejerLuk.Infrastructure.Config
{
    public static class KontoejerLukConfigurationFactory
    {
        public static JobConfiguration CreateRaw(string[] args)
        {
            var loader = new JobConfigurationLoader(new AzureAppConfigurationSettingsSource());
            return loader.Load(args);
        }

        public static KontoejerLukSettings CreateSettings(JobConfiguration configuration)
            => KontoejerLukSettings.Create(configuration);
    }
}
