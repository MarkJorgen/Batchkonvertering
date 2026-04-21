using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using NetConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;
using NetConfigurationRoot = Microsoft.Extensions.Configuration.IConfigurationRoot;

namespace Gi.Batch.Shared.Configuration
{
    public sealed class AzureAppConfigurationSettingsSource : IAzureSettingsSource
    {
        public IReadOnlyDictionary<string, string> Load(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string to Azure App Configuration is required.", nameof(connectionString));
            }

            var builder = new NetConfigurationBuilder();

            builder.AddAzureAppConfiguration(options =>
            {
                options.Connect(connectionString);
                options.Select(KeyFilter.Any, LabelFilter.Null);
            });

            NetConfigurationRoot configuration = builder.Build();

            return configuration
                .AsEnumerable()
                .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                .Where(x => x.Value != null)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value,
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}
