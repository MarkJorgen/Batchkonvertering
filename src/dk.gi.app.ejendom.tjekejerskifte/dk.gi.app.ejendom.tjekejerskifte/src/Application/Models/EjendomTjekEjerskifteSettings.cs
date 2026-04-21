using JobConfiguration = Gi.Batch.Shared.Configuration.JobConfiguration;
using dk.gi.app.ejendom.tjekejerskifte.Infrastructure.Config;

namespace dk.gi.app.ejendom.tjekejerskifte.Application.Models
{
    public sealed class EjendomTjekEjerskifteSettings
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
        public string ServiceBusBaseUrl { get; }
        public string ServiceBusSasKeyName { get; }
        public string ServiceBusSasKey { get; }
        public string ServiceBusQueueName { get; }
        public string ServiceBusLabel { get; }
        public string ServiceBusSessionId { get; }
        public int QueueScheduleStepSeconds { get; }
        public int MaxDage { get; }
        public int MaxAntal { get; }
        public bool VerifyCrmOnly { get; }
        public bool DryRun { get; }
        public bool RunMode { get; }

        private EjendomTjekEjerskifteSettings(string mode, bool enableLocalDebugLogging, string localDebugLogPath, string mutexName, int secondsToSleep, int maxWaitCount, int timeOutMinutes, bool reuseServiceClient, string failureRecipients, string crmConnectionTemplate, string crmServerName, string crmClientId, string crmClientSecret, string crmAuthority, string crmAuthorityMode, string serviceBusBaseUrl, string serviceBusSasKeyName, string serviceBusSasKey, string serviceBusQueueName, string serviceBusLabel, string serviceBusSessionId, int queueScheduleStepSeconds, int maxDage, int maxAntal, bool verifyCrmOnly, bool dryRun, bool runMode)
        {
            Mode = mode ?? string.Empty; EnableLocalDebugLogging = enableLocalDebugLogging; LocalDebugLogPath = localDebugLogPath ?? string.Empty; MutexName = mutexName ?? string.Empty; SecondsToSleep = secondsToSleep; MaxWaitCount = maxWaitCount; TimeOutMinutes = timeOutMinutes; ReuseServiceClient = reuseServiceClient; FailureRecipients = failureRecipients ?? string.Empty; CrmConnectionTemplate = crmConnectionTemplate ?? string.Empty; CrmServerName = crmServerName ?? string.Empty; CrmClientId = crmClientId ?? string.Empty; CrmClientSecret = crmClientSecret ?? string.Empty; CrmAuthority = crmAuthority ?? string.Empty; CrmAuthorityMode = crmAuthorityMode ?? string.Empty; ServiceBusBaseUrl = serviceBusBaseUrl ?? string.Empty; ServiceBusSasKeyName = serviceBusSasKeyName ?? string.Empty; ServiceBusSasKey = serviceBusSasKey ?? string.Empty; ServiceBusQueueName = serviceBusQueueName ?? string.Empty; ServiceBusLabel = serviceBusLabel ?? string.Empty; ServiceBusSessionId = serviceBusSessionId ?? string.Empty; QueueScheduleStepSeconds = queueScheduleStepSeconds; MaxDage = maxDage; MaxAntal = maxAntal; VerifyCrmOnly = verifyCrmOnly; DryRun = dryRun; RunMode = runMode;
        }
        public static EjendomTjekEjerskifteSettings Create(JobConfiguration configuration)
        {
            string mode = (configuration.Get("Mode", "DRYRUN") ?? "DRYRUN").Trim().ToUpperInvariant();
            bool verifyCrmOnly = mode == "VERIFYCRM";
            bool dryRun = mode == "DRYRUN" || mode == "SIMULATE";
            bool runMode = mode == "RUN" || mode == "FINDEJENDOMME";
            return new EjendomTjekEjerskifteSettings(mode, configuration.GetBool("EnableLocalDebugLogging"), configuration.Get("LocalDebugLogPath", string.Empty), configuration.Get("MutexName", "dk.gi.app.ejendom.tjekejerskifte"), configuration.GetInt("SecondsToSleep", 25), configuration.GetInt("MaxWaitCount", 5), configuration.GetInt("TimeOutMinutter", 2), configuration.GetBool("reuseserviceclient", true), configuration.Get("modtagereEmail", string.Empty), CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmConnectionTemplate", string.Empty)), CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmServerName", string.Empty)), CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmClientId", string.Empty)), CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmClientSecret", string.Empty)), CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmAuthority", string.Empty)), CrmScalarSettingNormalizer.Normalize(configuration.Get("AuthorityMode", configuration.Get("CrmAuthorityMode", "AsConfigured"))), CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusBaseUrl", string.Empty)), CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusSasKeyName", string.Empty)), CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusSasKey", string.Empty)), CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusQueueName", "crmpluginjobs")), CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusLabel", "TINGLYSNINGDATO")), CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusSessionId", string.Empty)), configuration.GetInt("QueueScheduleStepSeconds", 15), configuration.GetInt("MaxDage", 30), configuration.GetInt("MaxAntal", 1000), verifyCrmOnly, dryRun, runMode);
        }
    }
}
