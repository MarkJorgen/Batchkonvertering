using System;
using System.Collections.Generic;
using System.Configuration;
using dk.gi.app.konto.afslutarealsager.Application.Models;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Config
{
    public static class KontoAfslutArealSagerSettingsValidator
    {
        public static void Validate(KontoAfslutArealSagerSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(settings.Mode)) missing.Add("Mode");
            if (settings.SecondsToSleep < 0) throw new ConfigurationErrorsException("SecondsToSleep må ikke være negativ.");
            if (settings.MaxWaitCount < 0) throw new ConfigurationErrorsException("MaxWaitCount må ikke være negativ.");
            if (settings.TimeOutMinutes <= 0) throw new ConfigurationErrorsException("TimeOutMinutter skal være større end 0.");
            if (settings.EnableLocalDebugLogging && string.IsNullOrWhiteSpace(settings.LocalDebugLogPath)) missing.Add("LocalDebugLogPath");

            if (string.IsNullOrWhiteSpace(settings.CrmConnectionTemplate)) missing.Add("CrmConnectionTemplate");
            if (string.IsNullOrWhiteSpace(settings.CrmServerName)) missing.Add("CrmServerName");
            if (string.IsNullOrWhiteSpace(settings.CrmClientId)) missing.Add("CrmClientId");
            if (string.IsNullOrWhiteSpace(settings.CrmClientSecret)) missing.Add("CrmClientSecret");
            if (string.IsNullOrWhiteSpace(settings.CrmAuthority)) missing.Add("CrmAuthority");
            if (!settings.VerifyCrmOnly && !settings.EnableDiscoveryRun && string.IsNullOrWhiteSpace(settings.BrugerArealSager)) missing.Add("BrugerArealSager");
            if (settings.DiscoveryLimit <= 0) throw new ConfigurationErrorsException("DiscoveryLimit skal være større end 0.");
            if (settings.DiscoveryLimit > 200) throw new ConfigurationErrorsException("DiscoveryLimit må højst være 200.");
            if (settings.OpfoelgesFraPlusDage < -3650) throw new ConfigurationErrorsException("OpfoelgesFraPlusDage er åbenlyst ugyldig.");
            if (settings.EnableCloseoutQueueRun || settings.EnableArealSumQueueRun)
            {
                if (string.IsNullOrWhiteSpace(settings.ServiceBusQueueName)) missing.Add("ServiceBusQueueName");
            }
            if (settings.EnableCloseoutQueueRun && string.IsNullOrWhiteSpace(settings.ServiceBusLabel)) missing.Add("ServiceBusLabel");

            if (missing.Count > 0)
                throw new ConfigurationErrorsException("Følgende settings mangler til valgt mode: " + string.Join(", ", missing));
        }
    }
}
