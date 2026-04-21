using System.Collections.Generic;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Tests.Unit
{
    internal static class ContactRegistreringUdloebneOptaellingSettingsBuilder
    {
        public static ContactRegistreringUdloebneOptaellingSettings Build(
            string mode = "DRYRUN",
            string serviceBusBaseUrl = null,
            string serviceBusSasKeyName = null,
            string serviceBusSasKey = null,
            string serviceBusQueueName = null,
            string serviceBusLabel = null,
            string serviceBusSessionId = null,
            string crmAuthority = null,
            string crmAuthorityMode = "AsConfigured")
        {
            bool needsCrm = mode == "RUN" || mode == "VERIFYCRM";
            bool needsSb = mode == "RUN";
            var configuration = new JobConfiguration(new Dictionary<string, string>
            {
                ["Mode"] = mode,
                ["EnableLocalDebugLogging"] = "false",
                ["LocalDebugLogPath"] = "",
                ["MutexName"] = "test-mutex-" + mode,
                ["SecondsToSleep"] = "1",
                ["MaxWaitCount"] = "1",
                ["modtagereEmail"] = "test@gi.dk",
                ["CrmConnectionTemplate"] = needsCrm ? "AuthType=ClientSecret;Url=https://{0};ClientId={1};ClientSecret={2};Authority=https://{3};" : string.Empty,
                ["CrmServerName"] = needsCrm ? "server.crm4.dynamics.com/" : string.Empty,
                ["CrmClientId"] = needsCrm ? "client" : string.Empty,
                ["CrmClientSecret"] = needsCrm ? "secret" : string.Empty,
                ["CrmAuthority"] = crmAuthority ?? (needsCrm ? "authority" : string.Empty),
                ["CrmAuthorityMode"] = crmAuthorityMode,
                ["ServiceBusBaseUrl"] = serviceBusBaseUrl ?? (needsSb ? "https://namespace.servicebus.windows.net" : string.Empty),
                ["ServiceBusSasKeyName"] = serviceBusSasKeyName ?? (needsSb ? "RootManageSharedAccessKey" : string.Empty),
                ["ServiceBusSasKey"] = serviceBusSasKey ?? (needsSb ? "secret-key" : string.Empty),
                ["ServiceBusQueueName"] = serviceBusQueueName ?? string.Empty,
                ["ServiceBusLabel"] = serviceBusLabel ?? string.Empty,
                ["ServiceBusSessionId"] = serviceBusSessionId ?? string.Empty
            });
            return ContactRegistreringUdloebneOptaellingSettings.Create(configuration);
        }
    }
}
