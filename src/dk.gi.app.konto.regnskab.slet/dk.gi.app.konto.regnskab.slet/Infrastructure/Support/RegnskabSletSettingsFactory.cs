using System;
using System.Collections.Generic;
using dk.gi.app.konto.regnskab.slet.Application.Models;

namespace dk.gi.app.konto.regnskab.slet.Infrastructure.Support
{
    public static class RegnskabSletSettingsFactory
    {
        public static RegnskabSletSettings Create(IReadOnlyDictionary<string, string> settings, string[] args)
        {
            return new RegnskabSletSettings
            {
                Mode = ParseMode(Get(settings, "Mode", "DRYRUN")),
                AuthorityMode = Get(settings, "AuthorityMode", "AsConfigured"),
                RuntimeEngine = Get(settings, "RuntimeEngine", "Modern"),
                CrmConnectionTemplate = Get(settings, "CrmConnectionTemplate", "AuthType=ClientSecret;Url=https://{0};ClientId={1};ClientSecret={2};Authority=https://{3};RequireNewInstance=True;"),
                CrmServerName = Get(settings, "CrmServerName", string.Empty),
                CrmClientId = Get(settings, "CrmClientId", string.Empty),
                CrmClientSecret = Get(settings, "CrmClientSecret", string.Empty),
                CrmAuthority = Get(settings, "CrmAuthority", string.Empty),
                ServiceBusBaseUrl = Get(settings, "ServiceBusBaseUrl", string.Empty),
                ServiceBusSasKeyName = Get(settings, "ServiceBusSasKeyName", string.Empty),
                ServiceBusSasKey = Get(settings, "ServiceBusSasKey", string.Empty),
                ServiceBusQueueName = Get(settings, "ServiceBusQueueName", "crmpluginjobs"),
                ServiceBusLabel = Get(settings, "ServiceBusLabel", "KontoDiv"),
                ServiceBusSessionId = Get(settings, "ServiceBusSessionId", string.Empty),
                DelayStepSeconds = ParseInt(Get(settings, "DelayStepSeconds", 15), 15),
                DefaultBatchCount = ParseInt(Get(settings, "DefaultBatchCount", 100), 100),
                FailureRecipients = Get(settings, "modtagereEmail", string.Empty),
            };
        }

        private static string Get(IReadOnlyDictionary<string, string> settings, string key, string defaultValue)
        {
            return settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
        }

        private static int Get(IReadOnlyDictionary<string, string> settings, string key, int defaultValue)
        {
            return settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) && int.TryParse(value, out var result) ? result : defaultValue;
        }

        private static int ParseInt(int value, int defaultValue)
        {
            return value > 0 ? value : defaultValue;
        }

        private static JobExecutionMode ParseMode(string mode)
        {
            if (string.Equals(mode, "VERIFYCRM", StringComparison.OrdinalIgnoreCase)) return JobExecutionMode.VerifyCrm;
            if (string.Equals(mode, "RUN", StringComparison.OrdinalIgnoreCase)) return JobExecutionMode.Run;
            return JobExecutionMode.DryRun;
        }
    }
}
