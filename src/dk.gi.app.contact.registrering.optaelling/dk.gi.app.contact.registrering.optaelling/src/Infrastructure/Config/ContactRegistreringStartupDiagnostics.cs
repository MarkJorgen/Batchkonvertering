using System;
using System.Collections.Generic;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Crm;
using JobConfiguration = Gi.Batch.Shared.Configuration.JobConfiguration;

namespace dk.gi.app.contact.registrering.optaelling.Infrastructure.Config
{
    public static class ContactRegistreringStartupDiagnostics
    {
        public static IReadOnlyList<string> Build(JobConfiguration rawConfiguration, ContactRegistreringOptaellingSettings settings)
        {
            var lines = new List<string>
            {
                "[DIAG] Startup-diagnostik for contact.registrering.optaelling",
                "[DIAG] Mode=" + SafeValue(settings?.Mode),
                "[DIAG] UseConfigStore=" + SafeValue(rawConfiguration?.Get("UseConfigStore", "false")),
                "[DIAG] ConfigStoreConnectionString configured (legacy compat)=" + ToJaNej(HasValue(rawConfiguration, "ConfigStoreConnectionString")),
                "[DIAG] AZURE_APPCONFIG_CONNECTIONSTRING present=" + ToJaNej(HasValue(rawConfiguration, "AZURE_APPCONFIG_CONNECTIONSTRING")),
                "[DIAG] LocalOverrideFile=" + SafeValue(rawConfiguration?.Get("LocalOverrideFile", "appsettings.local.json")),
                "[DIAG] VerifyCrmOnly=" + ToJaNej(settings != null && settings.VerifyCrmOnly),
                "[DIAG] EnableLocalDebugLogging=" + ToJaNej(settings != null && settings.EnableLocalDebugLogging),
                "[DIAG] LocalDebugLogPath configured=" + ToJaNej(settings != null && !string.IsNullOrWhiteSpace(settings.LocalDebugLogPath)),
                "[DIAG] CrmConnectionTemplate=" + ToFoundMissing(settings != null && !string.IsNullOrWhiteSpace(settings.CrmConnectionTemplate)),
                "[DIAG] CrmServerName=" + ToFoundMissing(settings != null && !string.IsNullOrWhiteSpace(settings.CrmServerName)),
                "[DIAG] CrmClientId=" + ToFoundMissing(settings != null && !string.IsNullOrWhiteSpace(settings.CrmClientId)),
                "[DIAG] CrmClientSecret=" + ToFoundMissing(settings != null && !string.IsNullOrWhiteSpace(settings.CrmClientSecret)),
                "[DIAG] CrmAuthority=" + ToFoundMissing(settings != null && !string.IsNullOrWhiteSpace(settings.CrmAuthority)),
                "[DIAG] CrmAuthorityMode=" + SafeValue(settings?.CrmAuthorityMode),
                BuildNormalizationLine(rawConfiguration, settings, "CrmServerName", settings?.CrmServerName),
                BuildNormalizationLine(rawConfiguration, settings, "CrmClientId", settings?.CrmClientId),
                BuildNormalizationLine(rawConfiguration, settings, "CrmAuthority", settings?.CrmAuthority),
                BuildSecretNormalizationLine(rawConfiguration, settings)
            };

            return lines;
        }

        public static void WriteToConsole(IReadOnlyList<string> lines)
        {
            if (lines == null)
            {
                return;
            }

            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
        }

        private static string BuildNormalizationLine(JobConfiguration rawConfiguration, ContactRegistreringOptaellingSettings settings, string key, string normalizedValue)
        {
            string rawValue = rawConfiguration?.Get(key, string.Empty) ?? string.Empty;
            string normalized = normalizedValue ?? string.Empty;

            return "[DIAG] " + key + " normalized changed=" + ToJaNej(CrmScalarSettingNormalizer.WasNormalized(rawValue, normalized))
                + ", outer quotes raw=" + ToJaNej(CrmScalarSettingNormalizer.HasOuterQuotes(rawValue))
                + ", line breaks raw=" + ToJaNej(CrmScalarSettingNormalizer.ContainsLineBreaks(rawValue))
                + ", raw length=" + rawValue.Length
                + ", normalized length=" + normalized.Length;
        }

        private static string BuildSecretNormalizationLine(JobConfiguration rawConfiguration, ContactRegistreringOptaellingSettings settings)
        {
            string rawValue = rawConfiguration?.Get("CrmClientSecret", string.Empty) ?? string.Empty;
            string normalized = settings?.CrmClientSecret ?? string.Empty;
            string effective = CompatCrmSecretDecryptor.DecryptOrFallback(normalized, out bool decrypted);

            return "[DIAG] CrmClientSecret normalized changed=" + ToJaNej(CrmScalarSettingNormalizer.WasNormalized(rawValue, normalized))
                + ", outer quotes raw=" + ToJaNej(CrmScalarSettingNormalizer.HasOuterQuotes(rawValue))
                + ", line breaks raw=" + ToJaNej(CrmScalarSettingNormalizer.ContainsLineBreaks(rawValue))
                + ", looks like guid raw=" + ToJaNej(CrmScalarSettingNormalizer.LooksLikeGuid(rawValue))
                + ", looks like guid normalized=" + ToJaNej(CrmScalarSettingNormalizer.LooksLikeGuid(normalized))
                + ", compat decrypt applied=" + ToJaNej(decrypted)
                + ", raw length=" + rawValue.Length
                + ", normalized length=" + normalized.Length
                + ", effective length=" + effective.Length;
        }

        private static bool HasValue(JobConfiguration configuration, string key)
        {
            return configuration != null && !string.IsNullOrWhiteSpace(configuration.Get(key, string.Empty));
        }

        private static string ToFoundMissing(bool hasValue)
        {
            return hasValue ? "FOUND" : "MISSING";
        }

        private static string ToJaNej(bool value)
        {
            return value ? "Ja" : "Nej";
        }

        private static string SafeValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "<tom>" : value;
        }
    }
}
