using JobConfiguration = Gi.Batch.Shared.Configuration.JobConfiguration;
using dk.gi.app.konto.afslutarealsager.Infrastructure.Config;

namespace dk.gi.app.konto.afslutarealsager.Application.Models
{
    public sealed class KontoAfslutArealSagerSettings
    {
        public string Mode { get; }
        public bool EnableLocalDebugLogging { get; }
        public string LocalDebugLogPath { get; }
        public string MutexName { get; }
        public int SecondsToSleep { get; }
        public int MaxWaitCount { get; }
        public int TimeOutMinutes { get; }
        public bool ReuseServiceClient { get; }
        public string FailureRecipients { get; }
        public string CrmConnectionTemplate { get; }
        public string CrmServerName { get; }
        public string CrmClientId { get; }
        public string CrmClientSecret { get; }
        public string CrmAuthority { get; }
        public string CrmAuthorityMode { get; }
        public string BrugerArealSager { get; }
        public int OpfoelgesFraPlusDage { get; }
        public bool TilladSendTilDigitalPost { get; }
        public bool AllowPartialRun { get; }
        public bool EnableCloseoutQueueRun { get; }
        public bool EnableDirectIncidentCloseoutRun { get; }
        public bool EnableCarryForwardArealRun { get; }
        public bool EnableDeleteZeroRegnskabRun { get; }
        public bool EnableArealSumQueueRun { get; }
        public bool EnableDigitalPostStubRun { get; }
        public bool EnableDiscoveryRun { get; }
        public int DiscoveryLimit { get; }
        public int DirectIncidentCloseStatusCode { get; }
        public string ForceIncidentId { get; }
        public string ForceSagsnummer { get; }
        public string ForceKontonr { get; }
        public string ServiceBusBaseUrl { get; }
        public string ServiceBusSasKeyName { get; }
        public string ServiceBusSasKey { get; }
        public string ServiceBusQueueName { get; }
        public string ServiceBusLabel { get; }
        public string ServiceBusSessionId { get; }
        public bool VerifyCrmOnly { get; }
        public bool DryRun { get; }
        public bool RunMode { get; }

        private KontoAfslutArealSagerSettings(
            string mode,
            bool enableLocalDebugLogging,
            string localDebugLogPath,
            string mutexName,
            int secondsToSleep,
            int maxWaitCount,
            int timeOutMinutes,
            bool reuseServiceClient,
            string failureRecipients,
            string crmConnectionTemplate,
            string crmServerName,
            string crmClientId,
            string crmClientSecret,
            string crmAuthority,
            string crmAuthorityMode,
            string brugerArealSager,
            int opfoelgesFraPlusDage,
            bool tilladSendTilDigitalPost,
            bool allowPartialRun,
            bool enableCloseoutQueueRun,
            bool enableDirectIncidentCloseoutRun,
            bool enableCarryForwardArealRun,
            bool enableDeleteZeroRegnskabRun,
            bool enableArealSumQueueRun,
            bool enableDigitalPostStubRun,
            bool enableDiscoveryRun,
            int discoveryLimit,
            int directIncidentCloseStatusCode,
            string forceIncidentId,
            string forceSagsnummer,
            string forceKontonr,
            string serviceBusBaseUrl,
            string serviceBusSasKeyName,
            string serviceBusSasKey,
            string serviceBusQueueName,
            string serviceBusLabel,
            string serviceBusSessionId,
            bool verifyCrmOnly,
            bool dryRun,
            bool runMode)
        {
            Mode = mode ?? string.Empty;
            EnableLocalDebugLogging = enableLocalDebugLogging;
            LocalDebugLogPath = localDebugLogPath ?? string.Empty;
            MutexName = mutexName ?? string.Empty;
            SecondsToSleep = secondsToSleep;
            MaxWaitCount = maxWaitCount;
            TimeOutMinutes = timeOutMinutes;
            ReuseServiceClient = reuseServiceClient;
            FailureRecipients = failureRecipients ?? string.Empty;
            CrmConnectionTemplate = crmConnectionTemplate ?? string.Empty;
            CrmServerName = crmServerName ?? string.Empty;
            CrmClientId = crmClientId ?? string.Empty;
            CrmClientSecret = crmClientSecret ?? string.Empty;
            CrmAuthority = crmAuthority ?? string.Empty;
            CrmAuthorityMode = crmAuthorityMode ?? string.Empty;
            BrugerArealSager = brugerArealSager ?? string.Empty;
            OpfoelgesFraPlusDage = opfoelgesFraPlusDage;
            TilladSendTilDigitalPost = tilladSendTilDigitalPost;
            AllowPartialRun = allowPartialRun;
            EnableCloseoutQueueRun = enableCloseoutQueueRun;
            EnableDirectIncidentCloseoutRun = enableDirectIncidentCloseoutRun;
            EnableCarryForwardArealRun = enableCarryForwardArealRun;
            EnableDeleteZeroRegnskabRun = enableDeleteZeroRegnskabRun;
            EnableArealSumQueueRun = enableArealSumQueueRun;
            EnableDigitalPostStubRun = enableDigitalPostStubRun;
            EnableDiscoveryRun = enableDiscoveryRun;
            DiscoveryLimit = discoveryLimit;
            DirectIncidentCloseStatusCode = directIncidentCloseStatusCode;
            ForceIncidentId = forceIncidentId ?? string.Empty;
            ForceSagsnummer = forceSagsnummer ?? string.Empty;
            ForceKontonr = forceKontonr ?? string.Empty;
            ServiceBusBaseUrl = serviceBusBaseUrl ?? string.Empty;
            ServiceBusSasKeyName = serviceBusSasKeyName ?? string.Empty;
            ServiceBusSasKey = serviceBusSasKey ?? string.Empty;
            ServiceBusQueueName = serviceBusQueueName ?? string.Empty;
            ServiceBusLabel = serviceBusLabel ?? string.Empty;
            ServiceBusSessionId = serviceBusSessionId ?? string.Empty;
            VerifyCrmOnly = verifyCrmOnly;
            DryRun = dryRun;
            RunMode = runMode;
        }

        public static KontoAfslutArealSagerSettings Create(JobConfiguration configuration)
        {
            string mode = (configuration.Get("Mode", "DRYRUN") ?? "DRYRUN").Trim().ToUpperInvariant();
            bool verifyCrmOnly = mode == "VERIFYCRM";
            bool dryRun = mode == "DRYRUN" || mode == "SIMULATE";
            bool runMode = mode == "RUN";

            return new KontoAfslutArealSagerSettings(
                mode,
                configuration.GetBool("EnableLocalDebugLogging"),
                configuration.Get("LocalDebugLogPath", string.Empty),
                configuration.Get("MutexName", "dk.gi.app.konto.afslutarealsager"),
                configuration.GetInt("SecondsToSleep", 45),
                configuration.GetInt("MaxWaitCount", 15),
                configuration.GetInt("TimeOutMinutter", 2),
                configuration.GetBool("reuseserviceclient", true),
                configuration.Get("modtagereEmail", string.Empty),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmConnectionTemplate", string.Empty)),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmServerName", string.Empty)),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmClientId", string.Empty)),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmClientSecret", string.Empty)),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmAuthority", string.Empty)),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmAuthorityMode", "AsConfigured")),
                configuration.Get("BrugerArealSager", string.Empty),
                configuration.GetInt("OpfoelgesFraPlusDage", 0),
                configuration.GetBool("TilladSendTilDigitalPost"),
                configuration.GetBool("AllowPartialRun"),
                configuration.GetBool("EnableCloseoutQueueRun"),
                configuration.GetBool("EnableDirectIncidentCloseoutRun"),
                configuration.GetBool("EnableCarryForwardArealRun"),
                configuration.GetBool("EnableDeleteZeroRegnskabRun"),
                configuration.GetBool("EnableArealSumQueueRun"),
                configuration.GetBool("EnableDigitalPostStubRun"),
                configuration.GetBool("EnableDiscoveryRun"),
                configuration.GetInt("DiscoveryLimit", 20),
                configuration.GetInt("DirectIncidentCloseStatusCode", 5),
                configuration.Get("ForceIncidentId", string.Empty),
                configuration.Get("ForceSagsnummer", string.Empty),
                configuration.Get("ForceKontonr", string.Empty),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusBaseUrl", string.Empty)),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusSasKeyName", string.Empty)),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusSasKey", string.Empty)),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusQueueName", "crmpluginjobs")),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusLabel", "KontoDiv")),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusSessionId", string.Empty)),
                verifyCrmOnly,
                dryRun,
                runMode);
        }
    }
}
