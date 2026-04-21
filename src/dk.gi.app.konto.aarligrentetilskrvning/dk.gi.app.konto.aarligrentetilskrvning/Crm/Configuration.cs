using dk.gi.crm;
using dk.gi.crm.giproxy;
using dk.gi.crm.managers;
using dk.gi.crm.managers.V2;
using dk.gi.crm.models;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.app.konto.aarligrentetilskrvning
{
    public class Configuration
    {
        /// <summary>
        /// Opretter finanspost i CRM
        /// </summary>
        public static void Saet(ILogger trace, CrmContext crmcontext, Entity configurationSetting, string vaerdi)
        {
            trace.LogInformation($"Sætter configuration til {vaerdi} i CRM...");

            try
            {
                using (ConfigurationSettingsManager managerConfiguration = new ConfigurationSettingsManager(crmcontext))
                {
                    configurationSetting.Attributes["config_ntextcolumn"] = vaerdi;
                    managerConfiguration.Update(configurationSetting);
                }
            }
            catch (Exception ex)
            {
                trace.LogError($"Fejl ved opdatering af configurationSettings: app.konto.aarligrentetilskrvning.frakontonr til {vaerdi} {ex.Message}");
            }
        }
    }
}
