using System;
using System.Collections.Generic;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.konto.beregnsatserlog.slet.Infrastructure.Support
{
    public static class FlatJsonSettingsLoader
    {
        public static IReadOnlyDictionary<string, string> LoadMerged(string[] args)
        {
            var loader = new JobConfigurationLoader(new AzureAppConfigurationSettingsSource());
            JobConfiguration configuration = loader.Load(args ?? Array.Empty<string>());
            return configuration.Values;
        }
    }
}
