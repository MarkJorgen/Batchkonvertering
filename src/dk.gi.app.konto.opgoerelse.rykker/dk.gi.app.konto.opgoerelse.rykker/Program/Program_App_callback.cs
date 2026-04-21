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
namespace dk.gi.app.konto.opgoerelse.rykker
{
    /// <summary>
    /// Din kode skal tilføjes i Metoden Start her i dette objekt
    /// </summary>
    public partial class GIConsoleApp
    {
        AppStatus appStatus = new AppStatus();

        /// <summary>
        /// Her udføres den egentlige behandling
        /// </summary>
        /// <returns>AppStatus</returns>
        private void CallBackFunction()
        {
            Trace.LogInformation("CallBackFunction blev kaldt");

            //// ****************************************
            //// Indtast tekst her
            //// -MODE=XXXXXX Hvis mode er xxxxx så kaldes request xyz som henter/opdaterer/sletter eller .....
            //// ****************************************
            if (appConfig.Mode.ToUpper() == "BATCH")
            {
                DanKontoRykkerRequest danKontoRykkerRequest = new DanKontoRykkerRequest(crmcontext)
                {
                };
                DanKontoRykkerResponse danKontoRykkerResponse = danKontoRykkerRequest.Execute<DanKontoRykkerResponse>();

                if (danKontoRykkerResponse.Status.IsErrorOrWarning())
                    appStatus.SetStatus = AppStatus.StateCode.AppExceptionInCode;  // Din fejlkode som retuneres til OS -  lad den stå medmindre du ønsker et andet tal (bruges nedenfor til fejlhåndtering)
            }

            if (appConfig.Mode.ToUpper() == "YYYYYY")
            {
                // Do something here and get a response
                /// ....
                // If response status is not OK return error text message
                //if (response.Status.IsErrorOrWarning())
                //    appStatus.SetStatusTekstmsg = response.Status.Message;

            }

            Trace.LogInformation("CallBackFunction slut");
        }

        private void ModeXXX()
        {
            // Do something here and get a response
            /// ....
            // If response status is not OK return error text message
            //if (response.Status.IsErrorOrWarning())
            //    appStatus.SetStatusTekstmsg = response.Status.Message;
        }
    }
}