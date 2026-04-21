using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace dk.gi.app.konto.beregnsatserlog.slet.Infrastructure.Support
{
    public static class StartupDiagnosticsWriter
    {
        public static void Write(ILogger logger, string applicationName, IReadOnlyDictionary<string, string> settings)
        {
            string mode = Get(settings, "Mode", "DRYRUN");
            string authorityMode = Get(settings, "AuthorityMode", "AsConfigured");
            string runtimeEngine = Get(settings, "RuntimeEngine", "Modern");
            string crmServer = HasValue(settings, "CrmServerName") ? "FOUND" : "MISSING";
            string crmClientId = HasValue(settings, "CrmClientId") ? "FOUND" : "MISSING";
            string crmClientSecret = HasValue(settings, "CrmClientSecret") ? "FOUND" : "MISSING";
            string crmAuthority = HasValue(settings, "CrmAuthority") ? "FOUND" : "MISSING";

            WriteLine(logger, "[DIAG] Startup-diagnostik for " + applicationName);
            WriteLine(logger, "[DIAG] Mode=" + mode);
            WriteLine(logger, "[DIAG] AuthorityMode=" + authorityMode);
            WriteLine(logger, "[DIAG] RuntimeEngine=" + runtimeEngine);
            WriteLine(logger, "[DIAG] CrmServerName=" + crmServer);
            WriteLine(logger, "[DIAG] CrmClientId=" + crmClientId);
            WriteLine(logger, "[DIAG] CrmClientSecret=" + crmClientSecret);
            WriteLine(logger, "[DIAG] CrmAuthority=" + crmAuthority);
            WriteLine(logger, "[DIAG] AntalAar=" + Get(settings, "AntalAar", "3"));
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

        private static bool HasValue(IReadOnlyDictionary<string, string> settings, string key)
        {
            return settings != null && settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value);
        }
    }
}
