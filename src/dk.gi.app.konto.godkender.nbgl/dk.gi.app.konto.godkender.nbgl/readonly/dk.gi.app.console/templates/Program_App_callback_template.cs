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
namespace dk.gi.crm.app.konto.godkender.nbgl
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

            //// ****************************************
            //// Indtast tekst her
            //// -MODE=XXXXXX Hvis mode er xxxxx så kaldes request xyz som henter/opdaterer/sletter eller .....
            //// ****************************************
            if (appConfig.Mode.ToUpper() == "XXXXXX")
            {
                result = ModeXXX();
                    
            }
            if (appConfig.Mode.ToUpper() == "YYYYYY")
            {
                // Do something here and get a response
                /// ....
                // If response status is not OK return error text message
                //if (response.Status.IsErrorOrWarning())
                //    result.SetStatusTekstmsg = response.Status.Message;

            }

            Trace.LogInformation("CallBackFunction slut");
            return result;
        }

        private AppStatus ModeXXX()
        {
            AppStatus result = new AppStatus();
            // Do something here and get a response
            /// ....
            // If response status is not OK return error text message
            //if (response.Status.IsErrorOrWarning())
            //    result.SetStatusTekstmsg = response.Status.Message;
            return result;
        }
    }    
}