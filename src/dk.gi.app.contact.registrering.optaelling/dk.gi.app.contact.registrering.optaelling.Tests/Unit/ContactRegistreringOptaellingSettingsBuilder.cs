using System.Collections.Generic;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.contact.registrering.optaelling.Tests.Unit
{
    internal static class ContactRegistreringOptaellingSettingsBuilder
    {
        public static ContactRegistreringOptaellingSettings Build(
            string mode = "DRYRUN",
            bool enableLocalDebugLogging = false,
            string localDebugLogPath = "",
            string crmConnectionTemplate = null,
            string crmServerName = null,
            string crmClientId = null,
            string crmClientSecret = null,
            string crmAuthority = null,
            string crmAuthorityMode = "AsConfigured")
        {
            var configuration = new JobConfiguration(new Dictionary<string, string>
            {
                ["Mode"] = mode,
                ["EnableLocalDebugLogging"] = enableLocalDebugLogging ? "true" : "false",
                ["LocalDebugLogPath"] = localDebugLogPath,
                ["MutexName"] = "test-mutex-" + mode,
                ["SecondsToSleep"] = "1",
                ["MaxWaitCount"] = "1",
                ["modtagereEmail"] = "test@gi.dk",
                ["CrmConnectionTemplate"] = crmConnectionTemplate ?? (mode == "RUN" || mode == "VERIFYCRM" ? "template-{0}-{1}-{2}-{3}" : string.Empty),
                ["CrmServerName"] = crmServerName ?? (mode == "RUN" || mode == "VERIFYCRM" ? "server" : string.Empty),
                ["CrmClientId"] = crmClientId ?? (mode == "RUN" || mode == "VERIFYCRM" ? "client" : string.Empty),
                ["CrmClientSecret"] = crmClientSecret ?? (mode == "RUN" || mode == "VERIFYCRM" ? "secret" : string.Empty),
                ["CrmAuthority"] = crmAuthority ?? (mode == "RUN" || mode == "VERIFYCRM" ? "authority" : string.Empty),
                ["CrmAuthorityMode"] = crmAuthorityMode
            });

            return ContactRegistreringOptaellingSettings.Create(configuration);
        }
    }
}
