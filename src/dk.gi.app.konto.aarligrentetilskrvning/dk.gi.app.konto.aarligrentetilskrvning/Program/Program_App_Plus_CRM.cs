using dk.gi.crm.models;
using Microsoft.Extensions.Logging;
/// <summary>
/// Koden her er den del af klassen GIConsoleApp (Partial)
///
/// Version: 2022 26 10 
/// Sidste ændring: Tilføjet styring af crm client opstart settings
///
/// Når du opretter en ny applikation er det tanken at denne Program_App_Plus_CRM_template.cs kopieres til din app Program_App_Plus_CRM.cs (Og sættes til Compile)
/// Når du opgraderer NuGet Pakken "dk.gi.app.console.template.GiNugetSrc", så gentages kopieringen herover, men nu med overskriv (Har du rettet, så red dine rettelser først)
/// </summary>
using System;
using System.Configuration;

//namespace dk.gi.app.console.template
namespace dk.gi.app.konto.aarligrentetilskrvning
{
    /// <summary>
    /// Partial del
    /// </summary>
    public partial class GIConsoleApp
    {
        internal AzureBCModel AzureBC { get; set; } = new AzureBCModel();

        /// <summary>
        /// Skal vi bruge en forbindelse til CRM, så brug settings til programmet til at forbinde til CRM og sæt lokal variabel crmcontext som indeholder kontext for crm forbindelsen
        /// </summary>
        internal bool OpretCrmConnection()
        {
            bool result = false;
            // Pakker denne ind i try/catch fordi der muligvis skal udføres mere inden job afsluttes
            try
            {
                Trace.LogInformation($"Azure.BC configuration start");

                AzureBC.BaseUrl = ConfigurationManager.AppSettings["Azure.BC.BaseUrl"].Replace("\"", "");
                AzureBC.ClientId = Guid.Parse(ConfigurationManager.AppSettings["Azure.BC.ClientId"].Replace("\"", ""));
                AzureBC.ClientSecret = ConfigurationManager.AppSettings["Azure.BC.ClientSecret"].Replace("\"", "");
                AzureBC.QueryStringBaseUrl = ConfigurationManager.AppSettings["Azure.BC.QueryStringBaseUrl"].Replace("\"", "");
                AzureBC.RegnskabId = Guid.Parse(ConfigurationManager.AppSettings["Azure.BC.RegnskabId"].Replace("\"", ""));
                AzureBC.TenantId = Guid.Parse(ConfigurationManager.AppSettings["Azure.BC.TenantId"].Replace("\"", ""));

                Trace.LogInformation($"Azure.BC configuration slut");

                crmcontext = new dk.gi.crm.CrmContext(appConfig.GetCrmConnectionString);
                // Test blev CRM objekt oprettet, den kaster exception hvis der opstår fejl undervejs
                if (crmcontext != null)
                {
                    Trace.LogInformation($"Dynamics CRM Connection valid. Nu forbundet til CRM");
                    result = true;
                }
                else
                    Trace.LogInformation($"Fejl ikke forbundet til CRM");  // Der kastes exception hvis ikke det går godt, så denne burde aldrig blive tilfældet
            }
            catch (Exception)
            {
                Trace.LogWarning($"Fejl: Forbindelse til CRM kunne ikke dannes!");
            }
            return result;
        }
        internal dk.gi.crm.CrmContext crmcontext { get; private set; } = null;
    }
}
