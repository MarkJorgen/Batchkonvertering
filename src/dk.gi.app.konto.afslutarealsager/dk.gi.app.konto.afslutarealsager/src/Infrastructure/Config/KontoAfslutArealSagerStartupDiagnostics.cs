using System;
using System.Collections.Generic;
using dk.gi.app.konto.afslutarealsager.Application.Models;
using dk.gi.app.konto.afslutarealsager.Infrastructure.Crm.Dataverse;
using JobConfiguration = Gi.Batch.Shared.Configuration.JobConfiguration;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Config
{
    public static class KontoAfslutArealSagerStartupDiagnostics
    {
        public static IReadOnlyList<string> Build(JobConfiguration rawConfiguration, KontoAfslutArealSagerSettings settings)
        {
            return new List<string>
            {
                "[DIAG] Startup-diagnostik for konto.afslutarealsager",
                "[DIAG] Mode=" + SafeValue(settings?.Mode),
                "[DIAG] UseConfigStore=" + SafeValue(rawConfiguration?.Get("UseConfigStore", "<auto>")),
                "[DIAG] AZURE_APPCONFIG_CONNECTIONSTRING present=" + ToJaNej(HasValue(rawConfiguration, "AZURE_APPCONFIG_CONNECTIONSTRING")),
                "[DIAG] LocalOverrideFile=" + SafeValue(rawConfiguration?.Get("LocalOverrideFile", "appsettings.local.json")),
                "[DIAG] VerifyCrmOnly=" + ToJaNej(settings != null && settings.VerifyCrmOnly),
                "[DIAG] DryRun=" + ToJaNej(settings != null && settings.DryRun),
                "[DIAG] RunMode=" + ToJaNej(settings != null && settings.RunMode),
                "[DIAG] AllowPartialRun=" + ToJaNej(settings != null && settings.AllowPartialRun),
                "[DIAG] EnableCloseoutQueueRun=" + ToJaNej(settings != null && settings.EnableCloseoutQueueRun),
                "[DIAG] EnableDirectIncidentCloseoutRun=" + ToJaNej(settings != null && settings.EnableDirectIncidentCloseoutRun),
                "[DIAG] EnableCarryForwardArealRun=" + ToJaNej(settings != null && settings.EnableCarryForwardArealRun),
                "[DIAG] EnableDeleteZeroRegnskabRun=" + ToJaNej(settings != null && settings.EnableDeleteZeroRegnskabRun),
                "[DIAG] EnableArealSumQueueRun=" + ToJaNej(settings != null && settings.EnableArealSumQueueRun),
                "[DIAG] EnableDigitalPostStubRun=" + ToJaNej(settings != null && settings.EnableDigitalPostStubRun),
                "[DIAG] EnableDiscoveryRun=" + ToJaNej(settings != null && settings.EnableDiscoveryRun),
                "[DIAG] DiscoveryLimit=" + (settings != null ? settings.DiscoveryLimit.ToString() : "<tom>"),
                "[DIAG] ForceIncidentId=" + SafeValue(settings?.ForceIncidentId),
                "[DIAG] ForceSagsnummer=" + SafeValue(settings?.ForceSagsnummer),
                "[DIAG] ForceKontonr=" + SafeValue(settings?.ForceKontonr),
                "[DIAG] BrugerArealSager=" + SafeValue(settings?.BrugerArealSager),
                "[DIAG] OpfoelgesFraPlusDage=" + (settings != null ? settings.OpfoelgesFraPlusDage.ToString() : "<tom>"),
                "[DIAG] TilladSendTilDigitalPost=" + ToJaNej(settings != null && settings.TilladSendTilDigitalPost),
                "[DIAG] ServiceBusQueueName=" + SafeValue(settings?.ServiceBusQueueName),
                "[DIAG] ServiceBusLabel=" + SafeValue(settings?.ServiceBusLabel),
                "[DIAG] CrmServerName=" + ToFoundMissing(settings != null && !string.IsNullOrWhiteSpace(settings.CrmServerName)),
                "[DIAG] CrmClientId=" + ToFoundMissing(settings != null && !string.IsNullOrWhiteSpace(settings.CrmClientId)),
                "[DIAG] CrmClientSecret=" + ToFoundMissing(settings != null && !string.IsNullOrWhiteSpace(settings.CrmClientSecret)),
                "[DIAG] CrmAuthority=" + ToFoundMissing(settings != null && !string.IsNullOrWhiteSpace(settings.CrmAuthority)),
                BuildSecretNormalizationLine(rawConfiguration, settings)
            };
        }

        public static void WriteToConsole(IReadOnlyList<string> lines)
        {
            if (lines == null) return;
            foreach (var line in lines) Console.WriteLine(line);
        }

        private static string BuildSecretNormalizationLine(JobConfiguration rawConfiguration, KontoAfslutArealSagerSettings settings)
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
            => configuration != null && !string.IsNullOrWhiteSpace(configuration.Get(key, string.Empty));

        private static string ToFoundMissing(bool hasValue) => hasValue ? "FOUND" : "MISSING";
        private static string ToJaNej(bool value) => value ? "Ja" : "Nej";
        private static string SafeValue(string value) => string.IsNullOrWhiteSpace(value) ? "<tom>" : value;
    }
}
