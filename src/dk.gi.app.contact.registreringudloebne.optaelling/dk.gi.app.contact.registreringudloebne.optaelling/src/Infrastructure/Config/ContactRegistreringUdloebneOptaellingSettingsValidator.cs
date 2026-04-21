using System;
using System.Collections.Generic;
using System.Configuration;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Config
{
    public static class ContactRegistreringUdloebneOptaellingSettingsValidator
    {
        public static void Validate(ContactRegistreringUdloebneOptaellingSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(settings.Mode)) missing.Add("Mode");
            if (settings.SecondsToSleep < 0) throw new ConfigurationErrorsException("SecondsToSleep må ikke være negativ.");
            if (settings.MaxWaitCount < 0) throw new ConfigurationErrorsException("MaxWaitCount må ikke være negativ.");
            if (settings.EnableLocalDebugLogging && string.IsNullOrWhiteSpace(settings.LocalDebugLogPath)) missing.Add("LocalDebugLogPath");

            if (!settings.DryRun)
            {
                if (string.IsNullOrWhiteSpace(settings.CrmConnectionTemplate)) missing.Add("CrmConnectionTemplate");
                if (string.IsNullOrWhiteSpace(settings.CrmServerName)) missing.Add("CrmServerName");
                if (string.IsNullOrWhiteSpace(settings.CrmClientId)) missing.Add("CrmClientId");
                if (string.IsNullOrWhiteSpace(settings.CrmClientSecret)) missing.Add("CrmClientSecret");
                if (string.IsNullOrWhiteSpace(settings.CrmAuthority)) missing.Add("CrmAuthority");
            }

            // Service Bus settings må ikke valideres hårdt i startup.
            // BaseUrl/SAS kan resolves senere via CRM config_configurationsetting,
            // og Queue/Label/Session kan komme fra Config Store. Mangler håndteres derfor
            // først i selve publiceringssporet, hvis der faktisk er noget at sende.

            if (missing.Count > 0)
            {
                throw new ConfigurationErrorsException("Følgende settings mangler til valgt mode: " + string.Join(", ", missing));
            }
        }
    }
}
