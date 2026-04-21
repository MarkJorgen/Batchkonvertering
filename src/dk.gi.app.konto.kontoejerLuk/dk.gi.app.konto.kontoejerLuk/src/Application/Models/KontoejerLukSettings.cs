using JobConfiguration = Gi.Batch.Shared.Configuration.JobConfiguration;
using dk.gi.app.konto.kontoejerLuk.Infrastructure.Config;

namespace dk.gi.app.konto.kontoejerLuk.Application.Models
{
    public sealed class KontoejerLukSettings
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
        public bool VerifyCrmOnly { get; }
        public bool DryRun { get; }
        public bool RunMode { get; }

        private KontoejerLukSettings(
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
            VerifyCrmOnly = verifyCrmOnly;
            DryRun = dryRun;
            RunMode = runMode;
        }

        public static KontoejerLukSettings Create(JobConfiguration configuration)
        {
            string mode = (configuration.Get("Mode", "DRYRUN") ?? "DRYRUN").Trim().ToUpperInvariant();
            bool verifyCrmOnly = mode == "VERIFYCRM";
            bool dryRun = mode == "DRYRUN" || mode == "SIMULATE";
            bool runMode = mode == "RUN" || mode == "KONTOEJERLUK";

            return new KontoejerLukSettings(
                mode,
                configuration.GetBool("EnableLocalDebugLogging"),
                configuration.Get("LocalDebugLogPath", string.Empty),
                configuration.Get("MutexName", "dk.gi.app.konto.kontoejerLuk"),
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
                CrmScalarSettingNormalizer.Normalize(configuration.Get("AuthorityMode", configuration.Get("CrmAuthorityMode", "AsConfigured"))),
                verifyCrmOnly,
                dryRun,
                runMode);
        }
    }
}
