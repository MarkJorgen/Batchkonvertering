using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NetConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;
using NetConfigurationRoot = Microsoft.Extensions.Configuration.IConfigurationRoot;
using SystemConfigurationManager = System.Configuration.ConfigurationManager;

namespace Gi.Batch.Shared.Configuration
{
    public sealed class JobConfigurationLoader
    {
        private readonly IAzureSettingsSource _azureSettingsSource;

        public JobConfigurationLoader(IAzureSettingsSource azureSettingsSource)
        {
            _azureSettingsSource = azureSettingsSource ?? throw new ArgumentNullException(nameof(azureSettingsSource));
        }

        public JobConfiguration Load(string[] args)
        {
            var appConfigValues = ReadAppConfig();
            var environmentValues = ReadEnvironmentVariables();
            var argumentValues = ReadArguments(args);

            var bootstrap = Merge(appConfigValues, environmentValues, argumentValues);

            bool useConfigStore = ShouldUseConfigStore(appConfigValues, environmentValues, argumentValues);
            string configStoreConnectionString = FirstNonEmpty(
                Get(argumentValues, "AZURE_APPCONFIG_CONNECTIONSTRING"),
                Get(environmentValues, "AZURE_APPCONFIG_CONNECTIONSTRING"),
                Get(appConfigValues, "AZURE_APPCONFIG_CONNECTIONSTRING"),
                Get(argumentValues, "ConfigStoreConnectionString"),
                Get(environmentValues, "ConfigStoreConnectionString"),
                Get(appConfigValues, "ConfigStoreConnectionString"));
            string localOverrideFile = FirstNonEmpty(
                Get(bootstrap, "LocalOverrideFile"),
                "appsettings.local.json");

            var finalValues = new Dictionary<string, string>(appConfigValues, StringComparer.OrdinalIgnoreCase);

            if (useConfigStore)
            {
                if (string.IsNullOrWhiteSpace(configStoreConnectionString))
                {
                    throw new ConfigurationErrorsException(
                        "Config Store bootstrap er aktiveret, men AZURE_APPCONFIG_CONNECTIONSTRING er ikke sat. ConfigStoreConnectionString understøttes kun som legacy-kompatibilitet.");
                }

                MergeInto(finalValues, _azureSettingsSource.Load(configStoreConnectionString));
            }

            MergeIntoIgnoringEmpty(finalValues, environmentValues);

            string localOverridePath = ResolvePath(localOverrideFile);
            if (File.Exists(localOverridePath))
            {
                MergeIntoIgnoringEmpty(finalValues, ReadJsonFile(localOverridePath));
            }

            MergeInto(finalValues, argumentValues);

            return new JobConfiguration(finalValues);
        }

        private static Dictionary<string, string> ReadAppConfig()
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string key in SystemConfigurationManager.AppSettings.AllKeys)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                values[key] = SystemConfigurationManager.AppSettings[key];
            }

            return values;
        }

        private static Dictionary<string, string> ReadEnvironmentVariables()
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            IDictionary environmentVariables = Environment.GetEnvironmentVariables();

            foreach (DictionaryEntry entry in environmentVariables)
            {
                string key = entry.Key as string;
                string value = entry.Value as string;

                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                values[key] = value;
            }

            return values;
        }

        private static Dictionary<string, string> ReadArguments(string[] args)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (args == null)
            {
                return values;
            }

            foreach (string raw in args)
            {
                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                string candidate = raw.Trim();

                if (candidate.StartsWith("--"))
                {
                    candidate = candidate.Substring(2);
                }
                else if (candidate.StartsWith("-") || candidate.StartsWith("/"))
                {
                    candidate = candidate.Substring(1);
                }

                int separatorIndex = candidate.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = candidate.Substring(0, separatorIndex).Trim();
                string value = candidate.Substring(separatorIndex + 1).Trim();

                if (!string.IsNullOrWhiteSpace(key))
                {
                    values[key] = value;
                }
            }

            return values;
        }

        private static Dictionary<string, string> ReadJsonFile(string path)
        {
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            var builder = new NetConfigurationBuilder();

            if (Path.IsPathRooted(path))
            {
                builder
                    .SetBasePath(!string.IsNullOrWhiteSpace(directory) ? directory : AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile(fileName, optional: false, reloadOnChange: false);
            }
            else
            {
                builder
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile(fileName, optional: false, reloadOnChange: false);
            }

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

        private static string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.local.json");
            }

            return Path.IsPathRooted(path)
                ? path
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }

        private static Dictionary<string, string> Merge(params Dictionary<string, string>[] sources)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var source in sources)
            {
                MergeInto(result, source);
            }

            return result;
        }

        private static void MergeInto(Dictionary<string, string> target, IReadOnlyDictionary<string, string> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            foreach (var pair in source)
            {
                target[pair.Key] = pair.Value;
            }
        }

        private static void MergeIntoIgnoringEmpty(Dictionary<string, string> target, IReadOnlyDictionary<string, string> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            foreach (var pair in source)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(pair.Value))
                {
                    continue;
                }

                target[pair.Key] = pair.Value;
            }
        }

        private static string Get(IReadOnlyDictionary<string, string> values, string key)
        {
            string value;
            return values.TryGetValue(key, out value) ? value : null;
        }

        private static bool GetBool(IReadOnlyDictionary<string, string> values, string key)
        {
            bool parsed;
            return bool.TryParse(Get(values, key), out parsed) && parsed;
        }

        private static bool ShouldUseConfigStore(
            IReadOnlyDictionary<string, string> appConfigValues,
            IReadOnlyDictionary<string, string> environmentValues,
            IReadOnlyDictionary<string, string> argumentValues)
        {
            bool parsed;
            string explicitArgumentValue = Get(argumentValues, "UseConfigStore");
            if (!string.IsNullOrWhiteSpace(explicitArgumentValue) && bool.TryParse(explicitArgumentValue, out parsed))
            {
                return parsed;
            }

            string explicitEnvironmentValue = Get(environmentValues, "UseConfigStore");
            if (!string.IsNullOrWhiteSpace(explicitEnvironmentValue) && bool.TryParse(explicitEnvironmentValue, out parsed))
            {
                return parsed;
            }

            if (!string.IsNullOrWhiteSpace(Get(argumentValues, "AZURE_APPCONFIG_CONNECTIONSTRING"))
                || !string.IsNullOrWhiteSpace(Get(environmentValues, "AZURE_APPCONFIG_CONNECTIONSTRING"))
                || !string.IsNullOrWhiteSpace(Get(appConfigValues, "AZURE_APPCONFIG_CONNECTIONSTRING"))
                || !string.IsNullOrWhiteSpace(Get(argumentValues, "ConfigStoreConnectionString"))
                || !string.IsNullOrWhiteSpace(Get(environmentValues, "ConfigStoreConnectionString"))
                || !string.IsNullOrWhiteSpace(Get(appConfigValues, "ConfigStoreConnectionString")))
            {
                return true;
            }

            string explicitAppConfigValue = Get(appConfigValues, "UseConfigStore");
            if (!string.IsNullOrWhiteSpace(explicitAppConfigValue) && bool.TryParse(explicitAppConfigValue, out parsed))
            {
                return parsed;
            }

            return false;
        }

        private static string FirstNonEmpty(params string[] candidates)
        {
            return candidates.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        }
    }
}
