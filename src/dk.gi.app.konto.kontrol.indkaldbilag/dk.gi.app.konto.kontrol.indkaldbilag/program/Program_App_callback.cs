/// <summary>
/// Koden her er den del af klassen GIConsoleApp (Partial)
/// 
/// Version: 2022 12 19
/// Sidste ændring: Changed Result pattern to be AppStatus and not AppStatus.StateCode
///
/// Det er primært i denne at du skal rette, her skal den primære aktuelle program kode lægges
/// </summary>

using System;
using Microsoft.Extensions.Logging;
using dk.gi;
using dk.gi.crm.konto.forretning;
using dk.gi.crm.request.V2;

//namespace dk.gi.app.console.template
namespace dk.gi.crm.app.konto.kontrol.indkaldbilag
{
    /// <summary>
    /// Din kode skal tilføjes i Metoden Start her i dette objekt
    /// </summary>
    public partial class GIConsoleApp
    {
        /// <summary>
        /// Her udføres den egentlige behandling
        /// </summary>
        /// <returns>AppStatus</returns>
        private AppStatus CallBackFunction()
        {
            AppStatus result = new AppStatus();
            Trace.LogInformation("CallBackFunction blev kaldt");

            Trace.LogInformation($"Indkald kontrol bilag...");

            GenericStringResponse hentKontrolIndkaldBilagResponse = new GenericStringResponse();
            try
            {
                Trace.LogInformation("DoKontoKontrolIndkaldBilag start");

                HentKontrolIndkaldBilagRequest hentKontrolIndkaldBilagRequest = new HentKontrolIndkaldBilagRequest(crmcontext)
                {
                };
                hentKontrolIndkaldBilagResponse = hentKontrolIndkaldBilagRequest.Execute<GenericStringResponse>();

                if (hentKontrolIndkaldBilagResponse == null || hentKontrolIndkaldBilagResponse.Status == null)
                {
                    hentKontrolIndkaldBilagResponse = new GenericStringResponse();
                    hentKontrolIndkaldBilagResponse.SystemErrorMessage = "No response from DoKontoKontrolIndkaldBilag!";
                }
            }
            catch (Exception ex)
            {
                string str = $"Exception DoKontoKontrolIndkaldBilag:{ex}";
                hentKontrolIndkaldBilagResponse.SystemErrorMessage = str;
            }

            Trace.LogInformation("DoKontoKontrolIndkaldBilag slut");

            Trace.LogInformation("CallBackFunction slut");
            return result;
        }
    }
}