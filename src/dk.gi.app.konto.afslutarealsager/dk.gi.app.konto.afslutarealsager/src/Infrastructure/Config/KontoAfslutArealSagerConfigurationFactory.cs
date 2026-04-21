using dk.gi.app.konto.afslutarealsager.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Config
{
    public static class KontoAfslutArealSagerConfigurationFactory
    {
        public static JobConfiguration CreateRaw(string[] args)
        {
            var loader = new JobConfigurationLoader(new AzureAppConfigurationSettingsSource());
            return loader.Load(args);
        }

        public static KontoAfslutArealSagerSettings CreateSettings(JobConfiguration configuration)
        {
            return KontoAfslutArealSagerSettings.Create(configuration);
        }
    }
}
