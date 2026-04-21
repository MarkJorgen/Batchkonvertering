using System;
using System.Collections.Generic;
using JobConfiguration = Gi.Batch.Shared.Configuration.JobConfiguration;
using dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Config;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Application.Models
{
    public sealed class ContactRegistreringUdloebneOptaellingSettings
    {
        public string Mode { get; }
        public bool EnableLocalDebugLogging { get; }
        public string LocalDebugLogPath { get; }
        public string MutexName { get; }
        public int SecondsToSleep { get; }
        public int MaxWaitCount { get; }
        public string FailureRecipients { get; }
        public string CrmConnectionTemplate { get; }
        public string CrmServerName { get; }
        public string CrmClientId { get; }
        public string CrmClientSecret { get; }
        public string CrmAuthority { get; }
        public string CrmAuthorityMode { get; }
        public string ServiceBusBaseUrl { get; }
        public string ServiceBusSasKeyName { get; }
        public string ServiceBusSasKey { get; }
        public string ServiceBusQueueName { get; }
        public string ServiceBusLabel { get; }
        public string ServiceBusSessionId { get; }
        public bool DryRun { get; }
        public bool VerifyCrmOnly { get; }

        private ContactRegistreringUdloebneOptaellingSettings(
            string mode,
            bool enableLocalDebugLogging,
            string localDebugLogPath,
            string mutexName,
            int secondsToSleep,
            int maxWaitCount,
            string failureRecipients,
            string crmConnectionTemplate,
            string crmServerName,
            string crmClientId,
            string crmClientSecret,
            string crmAuthority,
            string crmAuthorityMode,
            string serviceBusBaseUrl,
            string serviceBusSasKeyName,
            string serviceBusSasKey,
            string serviceBusQueueName,
            string serviceBusLabel,
            string serviceBusSessionId)
        {
            Mode = mode;
            EnableLocalDebugLogging = enableLocalDebugLogging;
            LocalDebugLogPath = localDebugLogPath;
            MutexName = mutexName;
            SecondsToSleep = secondsToSleep;
            MaxWaitCount = maxWaitCount;
            FailureRecipients = failureRecipients;
            CrmConnectionTemplate = crmConnectionTemplate;
            CrmServerName = crmServerName;
            CrmClientId = crmClientId;
            CrmClientSecret = crmClientSecret;
            CrmAuthority = crmAuthority;
            CrmAuthorityMode = crmAuthorityMode;
            ServiceBusBaseUrl = serviceBusBaseUrl;
            ServiceBusSasKeyName = serviceBusSasKeyName;
            ServiceBusSasKey = serviceBusSasKey;
            ServiceBusQueueName = serviceBusQueueName;
            ServiceBusLabel = serviceBusLabel;
            ServiceBusSessionId = serviceBusSessionId;
            DryRun = string.Equals(mode, "DRYRUN", System.StringComparison.OrdinalIgnoreCase);
            VerifyCrmOnly = string.Equals(mode, "VERIFYCRM", System.StringComparison.OrdinalIgnoreCase);
        }

        public static ContactRegistreringUdloebneOptaellingSettings Create(JobConfiguration configuration)
        {
            string serviceBusBaseUrl = Normalize(FirstNonEmpty(
                configuration.Get("ServiceBusBaseUrl", string.Empty),
                GetServiceBusBaseUrlFromConnectionString(configuration)));

            string serviceBusSasKeyName = Normalize(FirstNonEmpty(
                configuration.Get("ServiceBusSasKeyName", string.Empty),
                GetServiceBusSasKeyNameFromConnectionString(configuration)));

            string serviceBusSasKey = Normalize(FirstNonEmpty(
                configuration.Get("ServiceBusSasKey", string.Empty),
                GetServiceBusSasKeyFromConnectionString(configuration)));

            return new ContactRegistreringUdloebneOptaellingSettings(
                mode: configuration.Get("Mode", "DRYRUN"),
                enableLocalDebugLogging: configuration.GetBool("EnableLocalDebugLogging", false),
                localDebugLogPath: configuration.Get("LocalDebugLogPath", string.Empty),
                mutexName: configuration.Get("MutexName", "dk.gi.app.contact.registreringudloebne.optaelling"),
                secondsToSleep: configuration.GetInt("SecondsToSleep", 25),
                maxWaitCount: configuration.GetInt("MaxWaitCount", 5),
                failureRecipients: FirstNonEmpty(
                    configuration.Get("modtagereEmail", string.Empty),
                    configuration.Get("Values:modtagereEmail", string.Empty)),
                crmConnectionTemplate: Normalize(configuration.Get("CrmConnectionTemplate", string.Empty)),
                crmServerName: Normalize(configuration.Get("CrmServerName", string.Empty)),
                crmClientId: Normalize(configuration.Get("CrmClientId", string.Empty)),
                crmClientSecret: Normalize(configuration.Get("CrmClientSecret", string.Empty)),
                crmAuthority: Normalize(configuration.Get("CrmAuthority", string.Empty)),
                crmAuthorityMode: configuration.Get("CrmAuthorityMode", "AsConfigured"),
                serviceBusBaseUrl: serviceBusBaseUrl,
                serviceBusSasKeyName: serviceBusSasKeyName,
                serviceBusSasKey: serviceBusSasKey,
                serviceBusQueueName: FirstNonEmpty(
                    configuration.Get("ServiceBusQueueName", string.Empty),
                    configuration.Get("Values:ServiceBusQueueName", string.Empty),
                    configuration.Get("Values:serviceBusQueueName", string.Empty)),
                serviceBusLabel: FirstNonEmpty(
                    configuration.Get("ServiceBusLabel", string.Empty),
                    configuration.Get("Values:ServiceBusLabel", string.Empty),
                    configuration.Get("Values:serviceBusLabel", string.Empty)),
                serviceBusSessionId: FirstNonEmpty(
                    configuration.Get("ServiceBusSessionId", string.Empty),
                    configuration.Get("Values:ServiceBusSessionId", string.Empty),
                    configuration.Get("Values:serviceBusSessionId", string.Empty)));
        }

        private static string GetServiceBusBaseUrlFromConnectionString(JobConfiguration configuration)
        {
            string connectionString = GetServiceBusConnectionString(configuration);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return string.Empty;
            }

            var parsed = ParseConnectionString(connectionString);
            string endpoint;
            if (!parsed.TryGetValue("Endpoint", out endpoint) || string.IsNullOrWhiteSpace(endpoint))
            {
                return string.Empty;
            }

            endpoint = endpoint.Trim();
            if (endpoint.EndsWith(";", StringComparison.Ordinal))
            {
                endpoint = endpoint.Substring(0, endpoint.Length - 1);
            }

            if (endpoint.StartsWith("sb://", StringComparison.OrdinalIgnoreCase))
            {
                return "https://" + endpoint.Substring("sb://".Length).TrimEnd('/');
            }

            return endpoint.TrimEnd('/');
        }

        private static string GetServiceBusSasKeyNameFromConnectionString(JobConfiguration configuration)
        {
            var parsed = ParseConnectionString(GetServiceBusConnectionString(configuration));
            string value;
            return parsed.TryGetValue("SharedAccessKeyName", out value) ? value : string.Empty;
        }

        private static string GetServiceBusSasKeyFromConnectionString(JobConfiguration configuration)
        {
            var parsed = ParseConnectionString(GetServiceBusConnectionString(configuration));
            string value;
            return parsed.TryGetValue("SharedAccessKey", out value) ? value : string.Empty;
        }

        private static string GetServiceBusConnectionString(JobConfiguration configuration)
        {
            return FirstNonEmpty(
                configuration.Get("serviceBusConnectionString", string.Empty),
                configuration.Get("ServiceBusConnectionString", string.Empty),
                configuration.Get("Values:serviceBusConnectionString", string.Empty),
                configuration.Get("Values:ServiceBusConnectionString", string.Empty));
        }

        private static Dictionary<string, string> ParseConnectionString(string connectionString)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return result;
            }

            string[] pairs = connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string pair in pairs)
            {
                int idx = pair.IndexOf('=');
                if (idx <= 0)
                {
                    continue;
                }

                string key = pair.Substring(0, idx).Trim();
                string value = pair.Substring(idx + 1).Trim();
                if (!string.IsNullOrWhiteSpace(key))
                {
                    result[key] = value;
                }
            }

            return result;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static string Normalize(string value)
        {
            return CrmScalarSettingNormalizer.Normalize(value);
        }
    }
}
