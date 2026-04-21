using System;
using System.Collections.Generic;
using dk.gi.app.contact.selskab.Application.Models;
using dk.gi.app.contact.selskab.Infrastructure.Crm;
using JobConfiguration = Gi.Batch.Shared.Configuration.JobConfiguration;

namespace dk.gi.app.contact.selskab.Infrastructure.Config
{
    public static class ContactSelskabStartupDiagnostics
    {
        public static IReadOnlyList<string> Build(JobConfiguration rawConfiguration, ContactSelskabSettings settings)
        {
            return new List<string>
            {
                "[DIAG] Startup-diagnostik for contact.selskab",
                "[DIAG] Mode=" + SafeValue(settings?.Mode),
                "[DIAG] UseConfigStore=" + SafeValue(rawConfiguration?.Get("UseConfigStore", "<auto>")),
                "[DIAG] AZURE_APPCONFIG_CONNECTIONSTRING present=" + ToJaNej(HasValue(rawConfiguration, "AZURE_APPCONFIG_CONNECTIONSTRING")),
                "[DIAG] LocalOverrideFile=" + SafeValue(rawConfiguration?.Get("LocalOverrideFile", "appsettings.local.json")),
                "[DIAG] VerifyCrmOnly=" + ToJaNej(settings != null && settings.VerifyCrmOnly),
                "[DIAG] DryRun=" + ToJaNej(settings != null && settings.DryRun),
                "[DIAG] RunMode=" + ToJaNej(settings != null && settings.RunMode),
                "[DIAG] CrmServerName=" + ToFoundMissing(settings != null && !string.IsNullOrWhiteSpace(settings.CrmServerName)),
                "[DIAG] CrmClientId=" + ToFoundMissing(settings != null && !string.IsNullOrWhiteSpace(settings.CrmClientId)),
                "[DIAG] CrmClientSecret=" + ToFoundMissing(settings != null && !string.IsNullOrWhiteSpace(settings.CrmClientSecret)),
                "[DIAG] CrmAuthority=" + ToFoundMissing(settings != null && !string.IsNullOrWhiteSpace(settings.CrmAuthority)),
                "[DIAG] ServiceBusQueueName=" + SafeValue(settings?.ServiceBusQueueName),
                "[DIAG] ServiceBusLabel=" + SafeValue(settings?.ServiceBusLabel),
                "[DIAG] ServiceBusSessionId=" + SafeValue(settings?.ServiceBusSessionId),
                BuildSecretNormalizationLine(rawConfiguration, settings)
            };
        }

        public static void WriteToConsole(IReadOnlyList<string> lines)
        {
            if (lines == null) return;
            foreach (var line in lines) Console.WriteLine(line);
        }

        private static string BuildSecretNormalizationLine(JobConfiguration rawConfiguration, ContactSelskabSettings settings)
        {
            string rawValue = rawConfiguration?.Get("CrmClientSecret", string.Empty) ?? string.Empty;
            string normalized = settings?.CrmClientSecret ?? string.Empty;
            string effective = CompatCrmSecretDecryptor.DecryptOrFallback(normalized, out bool decrypted);

            return "[DIAG] CrmClientSecret normalized changed=" + ToJaNej(CrmScalarSettingNormalizer.WasNormalized(rawValue, normalized))
                + ", compat decrypt applied=" + ToJaNej(decrypted)
                + ", raw length=" + rawValue.Length
                + ", normalized length=" + normalized.Length
                + ", effective length=" + effective.Length;
        }

        private static bool HasValue(JobConfiguration configuration, string key)
        {
            return configuration != null && !string.IsNullOrWhiteSpace(configuration.Get(key, string.Empty));
        }

        private static string ToFoundMissing(bool hasValue) => hasValue ? "FOUND" : "MISSING";
        private static string ToJaNej(bool value) => value ? "Ja" : "Nej";
        private static string SafeValue(string value) => string.IsNullOrWhiteSpace(value) ? "<tom>" : value;
    }
}
