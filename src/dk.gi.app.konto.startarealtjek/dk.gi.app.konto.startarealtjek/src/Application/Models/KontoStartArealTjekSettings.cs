using JobConfiguration = Gi.Batch.Shared.Configuration.JobConfiguration;
using dk.gi.app.konto.startarealtjek.Infrastructure.Config;

namespace dk.gi.app.konto.startarealtjek.Application.Models
{
    public sealed class KontoStartArealTjekSettings
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
        public int DefaultArealCheckYears { get; }
        public int DefaultBuildYearBefore { get; }
        public int DefaultBatchCountAlmindeligUdlejning { get; }
        public int DefaultBatchCountEjerforening { get; }
        public int DefaultBatchCountAndelsbolig { get; }
        public bool VerifyCrmOnly { get; }
        public bool DryRun { get; }
        public bool RunMode { get; }

        private KontoStartArealTjekSettings(
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
            string serviceBusBaseUrl,
            string serviceBusSasKeyName,
            string serviceBusSasKey,
            string serviceBusQueueName,
            string serviceBusLabel,
            string serviceBusSessionId,
            int queueScheduleStepSeconds,
            int defaultArealCheckYears,
            int defaultBuildYearBefore,
            int defaultBatchCountAlmindeligUdlejning,
            int defaultBatchCountEjerforening,
            int defaultBatchCountAndelsbolig,
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
            ServiceBusBaseUrl = serviceBusBaseUrl ?? string.Empty;
            ServiceBusSasKeyName = serviceBusSasKeyName ?? string.Empty;
            ServiceBusSasKey = serviceBusSasKey ?? string.Empty;
            ServiceBusQueueName = serviceBusQueueName ?? string.Empty;
            ServiceBusLabel = serviceBusLabel ?? string.Empty;
            ServiceBusSessionId = serviceBusSessionId ?? string.Empty;
            QueueScheduleStepSeconds = queueScheduleStepSeconds;
            DefaultArealCheckYears = defaultArealCheckYears;
            DefaultBuildYearBefore = defaultBuildYearBefore;
            DefaultBatchCountAlmindeligUdlejning = defaultBatchCountAlmindeligUdlejning;
            DefaultBatchCountEjerforening = defaultBatchCountEjerforening;
            DefaultBatchCountAndelsbolig = defaultBatchCountAndelsbolig;
            VerifyCrmOnly = verifyCrmOnly;
            DryRun = dryRun;
            RunMode = runMode;
        }

        public static KontoStartArealTjekSettings Create(JobConfiguration configuration)
        {
            string mode = (configuration.Get("Mode", "DRYRUN") ?? "DRYRUN").Trim().ToUpperInvariant();
            bool verifyCrmOnly = mode == "VERIFYCRM";
            bool dryRun = mode == "DRYRUN" || mode == "SIMULATE";
            bool runMode = mode == "RUN";

            return new KontoStartArealTjekSettings(
                mode,
                configuration.GetBool("EnableLocalDebugLogging"),
                configuration.Get("LocalDebugLogPath", string.Empty),
                configuration.Get("MutexName", "dk.gi.app.konto.startarealtjek"),
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
                CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusBaseUrl", string.Empty)),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusSasKeyName", string.Empty)),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusSasKey", string.Empty)),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusQueueName", "crmpluginjobs")),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusLabel", "ArealTjekKonto")),
                CrmScalarSettingNormalizer.Normalize(configuration.Get("ServiceBusSessionId", string.Empty)),
                configuration.GetInt("QueueScheduleStepSeconds", 15),
                configuration.GetInt("app.konto.arealtjek.antalaar", 3),
                configuration.GetInt("BuildYearBefore", 1970),
                configuration.GetInt("app.konto.arealtjek.antalprbatch.au", 0),
                configuration.GetInt("app.konto.arealtjek.antalprbatch.ef", 0),
                configuration.GetInt("app.konto.arealtjek.antalprbatch.ab", 0),
                verifyCrmOnly,
                dryRun,
                runMode);
        }
    }
}
