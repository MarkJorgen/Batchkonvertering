using System;
using System.Collections.Generic;
using System.Configuration;
using dk.gi.app.konto.startarealtjek.Application.Models;

namespace dk.gi.app.konto.startarealtjek.Infrastructure.Config
{
    public static class KontoStartArealTjekSettingsValidator
    {
        public static void Validate(KontoStartArealTjekSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(settings.Mode)) missing.Add("Mode");
            if (settings.SecondsToSleep < 0) throw new ConfigurationErrorsException("SecondsToSleep må ikke være negativ.");
            if (settings.MaxWaitCount < 0) throw new ConfigurationErrorsException("MaxWaitCount må ikke være negativ.");
            if (settings.TimeOutMinutes <= 0) throw new ConfigurationErrorsException("TimeOutMinutter skal være større end 0.");
            if (settings.EnableLocalDebugLogging && string.IsNullOrWhiteSpace(settings.LocalDebugLogPath)) missing.Add("LocalDebugLogPath");
            if (settings.QueueScheduleStepSeconds < 0) throw new ConfigurationErrorsException("QueueScheduleStepSeconds må ikke være negativ.");
            if (settings.DefaultArealCheckYears < 0) throw new ConfigurationErrorsException("app.konto.arealtjek.antalaar må ikke være negativ.");
            if (settings.DefaultBuildYearBefore <= 0) throw new ConfigurationErrorsException("BuildYearBefore skal være større end 0.");

            if (string.IsNullOrWhiteSpace(settings.CrmConnectionTemplate)) missing.Add("CrmConnectionTemplate");
            if (string.IsNullOrWhiteSpace(settings.CrmServerName)) missing.Add("CrmServerName");
            if (string.IsNullOrWhiteSpace(settings.CrmClientId)) missing.Add("CrmClientId");
            if (string.IsNullOrWhiteSpace(settings.CrmClientSecret)) missing.Add("CrmClientSecret");
            if (string.IsNullOrWhiteSpace(settings.CrmAuthority)) missing.Add("CrmAuthority");
            if (string.IsNullOrWhiteSpace(settings.ServiceBusQueueName)) missing.Add("ServiceBusQueueName");
            if (string.IsNullOrWhiteSpace(settings.ServiceBusLabel)) missing.Add("ServiceBusLabel");

            if (missing.Count > 0)
                throw new ConfigurationErrorsException("Følgende settings mangler til valgt mode: " + string.Join(", ", missing));
        }
    }
}
