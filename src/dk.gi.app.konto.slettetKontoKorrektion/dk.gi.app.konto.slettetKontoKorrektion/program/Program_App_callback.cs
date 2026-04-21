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

//namespace dk.gi.app.console.template
namespace dk.gi.crm.app.konto.slettetKontoKorrektion
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

            //if (appConfig.Mode.ToUpper() == "BATCH")
            //{
            SlettetKontoKorrektionRequest slettetKontoKorrektionRequest = new SlettetKontoKorrektionRequest(crmcontext)
            {
            };
            SlettetKontoKorrektionResponse slettetKontoKorrektionResponse = slettetKontoKorrektionRequest.Execute<SlettetKontoKorrektionResponse>();

            if (slettetKontoKorrektionResponse.Status.IsOK() == false)
            {
                result.SetStatus = AppStatus.StateCode.AppUventetFejlIProgramKode;
            }
            //}

            Trace.LogInformation("CallBackFunction slut");
            return result;
        }
    }
}