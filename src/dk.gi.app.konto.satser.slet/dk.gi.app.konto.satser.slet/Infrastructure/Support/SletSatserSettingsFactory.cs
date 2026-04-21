using System;
using System.Collections.Generic;
using dk.gi.app.konto.satser.slet.Application.Models;

namespace dk.gi.app.konto.satser.slet.Infrastructure.Support
{
    public static class SletSatserSettingsFactory
    {
        public static SletSatserSettings Create(IReadOnlyDictionary<string, string> settings, string[] args)
        {
            return new SletSatserSettings
            {
                Mode = ParseMode(Get(settings, "Mode", "DRYRUN")),
                AuthorityMode = Get(settings, "AuthorityMode", "AsConfigured"),
                RuntimeEngine = Get(settings, "RuntimeEngine", "Modern"),
                EnableLegacySettingWriteOut = ParseBool(Get(settings, "EnableLegacySettingWriteOut", "false")),
                SatsAar = ParseInt(Get(settings, "SatsAar", DateTime.Today.Year + 1), DateTime.Today.Year + 1),
                TimeOutMinutter = ParseInt(Get(settings, "TimeOutMinutter", 2), 2),
                SecondsToSleep = ParseInt(Get(settings, "SecondsToSleep", 45), 45),
                MaxWaitCount = ParseInt(Get(settings, "MaxWaitCount", 15), 15),
                CrmConnectionTemplate = Get(settings, "CrmConnectionTemplate", "AuthType=ClientSecret;Url=https://{0};ClientId={1};ClientSecret={2};Authority=https://{3};RequireNewInstance=True;"),
                CrmServerName = Get(settings, "CrmServerName", string.Empty),
                CrmClientId = Get(settings, "CrmClientId", string.Empty),
                CrmClientSecret = Get(settings, "CrmClientSecret", string.Empty),
                CrmAuthority = Get(settings, "CrmAuthority", string.Empty),
                CrmOrganisationName = Get(settings, "CrmOrganisationName", string.Empty),
                CrmUserName = Get(settings, "CrmUserName", string.Empty),
                CrmUserPassword = Get(settings, "CrmUserPassword", string.Empty),
                FailureRecipients = Get(settings, "modtagereEmail", string.Empty),
            };
        }

        private static string Get(IReadOnlyDictionary<string, string> settings, string key, string defaultValue)
        {
            return settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : defaultValue;
        }

        private static int Get(IReadOnlyDictionary<string, string> settings, string key, int defaultValue)
        {
            return settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) && int.TryParse(value, out var result)
                ? result
                : defaultValue;
        }

        private static int ParseInt(int value, int defaultValue)
        {
            return value > 0 ? value : defaultValue;
        }

        private static bool ParseBool(string value)
        {
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "ja", StringComparison.OrdinalIgnoreCase);
        }

        private static JobExecutionMode ParseMode(string mode)
        {
            if (string.Equals(mode, "VERIFYCRM", StringComparison.OrdinalIgnoreCase))
            {
                return JobExecutionMode.VerifyCrm;
            }

            if (string.Equals(mode, "RUN", StringComparison.OrdinalIgnoreCase))
            {
                return JobExecutionMode.Run;
            }

            return JobExecutionMode.DryRun;
        }
    }
}
