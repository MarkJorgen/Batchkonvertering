using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace dk.gi.app.konto.regnskab.slet.Infrastructure.Support
{
    public static class StartupDiagnosticsWriter
    {
        public static void Write(ILogger logger, string applicationName, IReadOnlyDictionary<string, string> settings)
        {
            WriteLine(logger, "[DIAG] Startup-diagnostik for " + applicationName);
            WriteLine(logger, "[DIAG] Mode=" + Get(settings, "Mode", "DRYRUN"));
            WriteLine(logger, "[DIAG] AuthorityMode=" + Get(settings, "AuthorityMode", "AsConfigured"));
            WriteLine(logger, "[DIAG] RuntimeEngine=" + Get(settings, "RuntimeEngine", "Modern"));
            WriteLine(logger, "[DIAG] CrmServerName=" + (HasValue(settings, "CrmServerName") ? "FOUND" : "MISSING"));
            WriteLine(logger, "[DIAG] CrmClientId=" + (HasValue(settings, "CrmClientId") ? "FOUND" : "MISSING"));
            WriteLine(logger, "[DIAG] CrmClientSecret=" + (HasValue(settings, "CrmClientSecret") ? "FOUND" : "MISSING"));
            WriteLine(logger, "[DIAG] CrmAuthority=" + (HasValue(settings, "CrmAuthority") ? "FOUND" : "MISSING"));
            WriteLine(logger, "[DIAG] ServiceBusQueueName=" + Get(settings, "ServiceBusQueueName", "crmpluginjobs"));
            WriteLine(logger, "[DIAG] ServiceBusLabel=" + Get(settings, "ServiceBusLabel", "KontoDiv"));
        }

        private static void WriteLine(ILogger logger, string line)
        {
            Console.WriteLine(line);
            logger?.LogInformation(line);
        }

        private static string Get(IReadOnlyDictionary<string, string> settings, string key, string defaultValue)
        {
            return settings != null && settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
        }

        private static bool HasValue(IReadOnlyDictionary<string, string> settings, string key)
        {
            return settings != null && settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value);
        }
    }
}
