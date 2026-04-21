/// <summary>
/// Koden her er den del af klassen GIConsoleApp (Partial)
/// 
/// Version: 2022 12 19
/// Sidste ændring: Changed Result pattern to be AppStatus and not AppStatus.StateCode
///
/// Når du opretter en ny applikation er det tanken at denne Program_App_template.cs kopieres til din app Program_App.cs (Og sættes til Compile)
/// Når du opgraderer NuGet Pakken "dk.gi.app.console.template.GiNugetSrc", så gentages kopieringen herover (Overskriver den eksisterende - Har du rettet, så red dine rettelser først) 
/// - Efterfølgende lægger du din kode ind i GIConsoleApp Start metoden nedenfor i denne
/// </summary>

//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
// Logging MEL Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
// GI
using dk.gi.app;
// 
// Tilføj Nuget pakke dk.gi.library eller dk.gi.GINugetSrc - for at få adgang til trace som bruges i using herunder samt GISerilogTrace
// 
using dk.gi;
//using dk.gi.crm.request;
//using dk.gi.crm.response;

//namespace dk.gi.app.console.template
namespace dk.gi.crm.laan.app
{
    /// <summary>
    /// Din kode skal tilføjes i Metoden Start her i dette objekt
    /// </summary>
    public partial class GIConsoleApp
    {
        /// <summary>
        /// Dette er her den egentlige program behandling skal udføres
        /// </summary>
        /// <returns></returns>
        internal AppStatus Start()
        {
            Program.WriteLineTempTraceLog("AppStatus.start start");
            AppStatus result = new AppStatus(); // Default resultat OK

            Trace.LogInformation(System.DateTime.Now.ToLongTimeString() + " GIConsoleApp.Start");

            // Husk at fjerne kommentarlinjer i funktionen OpretCrmConnection hvis du skal bruge CRM, samt fjerne kommentar fra this.OpretCrmConnection(); linjen nedenfor
            // Husk at fjerne kommentarlinjer i funktionen HentMSMQFraIndkomneKoe, FlytMessageToAfsluttet og FlytMessageToFejl hvis app er startet fra MSMQ trigger
            try
            {
                // ****************************************
                //   Program parametre(args), Har du tilføjet et eller flere parametre til program f.eks -MitTal=8 så får det fat i det med følgende: 
                //   string mittal = appConfig["MitTal"];
                //   * Bemærk: alle parametre er string, du må selv parse dem til ønsket type
                //   Test tilstde af flere parametre: if (appConfig.ContainsAll(new string[] { "param1", "param2"}) == true)
                //                                    else result.SetStatusManglendeParameter = "param1 eller param2";
                // ****************************************
                if (this.OpretCrmConnection() == true)   // Opret forbindelse til CRM, dette kaster exception hvis der ikke kunne forbindes, og det fanges i funktion og false retuners hertil
                {
                    // Udfør job, men vi vil ikke have for mange versioner af denne applikation kørende samtidigt, fordi .......
                    result = RunOrWaitForGoSignal(this.CallBackFunction, "dk.gi.app.laan.panteos.processkasserede", false);
                }
                else result.SetStatus = AppStatus.StateCode.AppIngenCRMForbindelse;
            }
            catch (Exception ex)
            {
                Program.WriteLineTempTraceLog("AppStatus.start exception 1");
                Trace.LogError("Der opstod en fejl i programmet: " + ex.Message);
                result.SetStatus = AppStatus.StateCode.AppExceptionInCode;
            }

            try
            {

                // Hvis behandling ikke gik godt, så er resultat ikke 0
                // - Hvis resultat ikke er 0, så kald rutine til flyt af Message i Message kø
                // - Hvis resultat ikke er 0, så kald rutine som sender mail med besked om at det ikke gik godt
                if (result.statecode != AppStatus.StateCode.OK)
            {
                if (appConfig.ContainsKey("msgID") == true)
                    this.FlytMessageToFejl(appConfig.msgID);
                // Send mail hvis det ikke går godt 
                if (appConfig.ValidateEmailConfigurationOgEmailModtager() == true)
                {
                    // Alle konfigurationssettings for at kunne sende mail var til stede, så kald kode som sender mail
                    this.SendEmail(result);
                }
            }
            else
            {
                // - Hvis resultat er 0, så kald rutine til flyt af Message i Message kø
                if (appConfig.ContainsKey("msgID") == true)
                    this.FlytMessageToAfsluttet(appConfig.msgID);
            }

            }
            catch (Exception)
            {
                Program.WriteLineTempTraceLog("AppStatus.start Exception 2");
                result.SetStatus = AppStatus.StateCode.AppUventetFejlIProgramKode;
            }

            Trace.LogInformation(System.DateTime.Now.ToLongTimeString() + " GIConsoleApp.Start færdig");
            Program.WriteLineTempTraceLog("AppStatus.start slut");
            return result;
        }
    }
}