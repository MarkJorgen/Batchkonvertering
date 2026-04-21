using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace dk.gi.app.konto.satser.opret.Infrastructure.Support
{
    public static class StartupDiagnosticsWriter
    {
        public static void Write(ILogger logger, string applicationName, IReadOnlyDictionary<string, string> settings)
        {
            WriteLine(logger, "[DIAG] Startup-diagnostik for " + applicationName);
            WriteLine(logger, "[DIAG] Mode=" + Get(settings, "Mode", "DRYRUN"));
            WriteLine(logger, "[DIAG] AuthorityMode=" + Get(settings, "AuthorityMode", "AsConfigured"));
            WriteLine(logger, "[DIAG] RuntimeEngine=" + Get(settings, "RuntimeEngine", "Modern"));
            WriteLine(logger, "[DIAG] SatsAar=" + Get(settings, "SatsAar", DateTime.Today.Year + 1 + ""));
            WriteLine(logger, "[DIAG] CrmSettingsConfigured=" + HasAll(settings));
        }

        private static void WriteLine(ILogger logger, string line)
        {
            Console.WriteLine(line);
            logger?.LogInformation(line);
        }

        private static string Get(IReadOnlyDictionary<string, string> settings, string key, string defaultValue)
        {
            if (settings != null && settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return defaultValue;
        }

        private static bool HasAll(IReadOnlyDictionary<string, string> settings)
        {
            return settings != null
                && settings.TryGetValue("CrmServerName", out var a) && !string.IsNullOrWhiteSpace(a)
                && settings.TryGetValue("CrmClientId", out var b) && !string.IsNullOrWhiteSpace(b)
                && settings.TryGetValue("CrmAuthority", out var c) && !string.IsNullOrWhiteSpace(c);
        }
    }
}
