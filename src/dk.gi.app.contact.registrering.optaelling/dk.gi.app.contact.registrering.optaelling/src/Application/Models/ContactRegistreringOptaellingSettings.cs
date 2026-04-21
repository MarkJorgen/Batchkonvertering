using JobConfiguration = Gi.Batch.Shared.Configuration.JobConfiguration;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Config;

namespace dk.gi.app.contact.registrering.optaelling.Application.Models
{
    public sealed class ContactRegistreringOptaellingSettings
    {
        public string Mode { get; }
        public bool EnableLocalDebugLogging { get; }
        public string LocalDebugLogPath { get; }
        public string MutexName { get; }
        public int SecondsToSleep { get; }
        public int MaxWaitCount { get; }
        public string FailureRecipients { get; }
        public string CrmConnectionTemplate { get; }
        public string CrmServerName { get; }
        public string CrmClientId { get; }
        public string CrmClientSecret { get; }
        public string CrmAuthority { get; }
        public string CrmAuthorityMode { get; }
        public bool DryRun { get; }
        public bool VerifyCrmOnly { get; }

        private ContactRegistreringOptaellingSettings(
            string mode,
            bool enableLocalDebugLogging,
            string localDebugLogPath,
            string mutexName,
            int secondsToSleep,
            int maxWaitCount,
            string failureRecipients,
            string crmConnectionTemplate,
            string crmServerName,
            string crmClientId,
            string crmClientSecret,
            string crmAuthority,
            string crmAuthorityMode)
        {
            Mode = mode;
            EnableLocalDebugLogging = enableLocalDebugLogging;
            LocalDebugLogPath = localDebugLogPath;
            MutexName = mutexName;
            SecondsToSleep = secondsToSleep;
            MaxWaitCount = maxWaitCount;
            FailureRecipients = failureRecipients;
            CrmConnectionTemplate = crmConnectionTemplate;
            CrmServerName = crmServerName;
            CrmClientId = crmClientId;
            CrmClientSecret = crmClientSecret;
            CrmAuthority = crmAuthority;
            CrmAuthorityMode = crmAuthorityMode;
            DryRun = string.Equals(mode, "DRYRUN", System.StringComparison.OrdinalIgnoreCase);
            VerifyCrmOnly = string.Equals(mode, "VERIFYCRM", System.StringComparison.OrdinalIgnoreCase);
        }

        public static ContactRegistreringOptaellingSettings Create(JobConfiguration configuration)
        {
            return new ContactRegistreringOptaellingSettings(
                mode: configuration.Get("Mode", "DRYRUN"),
                enableLocalDebugLogging: configuration.GetBool("EnableLocalDebugLogging", false),
                localDebugLogPath: configuration.Get("LocalDebugLogPath", string.Empty),
                mutexName: configuration.Get("MutexName", "dk.gi.app.contact.registrering.optaelling"),
                secondsToSleep: configuration.GetInt("SecondsToSleep", 25),
                maxWaitCount: configuration.GetInt("MaxWaitCount", 5),
                failureRecipients: configuration.Get("modtagereEmail", string.Empty),
                crmConnectionTemplate: CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmConnectionTemplate", string.Empty)),
                crmServerName: CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmServerName", string.Empty)),
                crmClientId: CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmClientId", string.Empty)),
                crmClientSecret: CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmClientSecret", string.Empty)),
                crmAuthority: CrmScalarSettingNormalizer.Normalize(configuration.Get("CrmAuthority", string.Empty)),
                crmAuthorityMode: configuration.Get("CrmAuthorityMode", "AsConfigured"));
        }
    }
}
