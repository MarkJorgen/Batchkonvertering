using Microsoft.Extensions.Logging;
/// <summary>
/// Koden her er den del af klassen GIConsoleApp (Partial)
///
/// Version: 2022 09 09
/// Sidste ændring: Tilpasset til Extensions Logging
/// 
/// Når du opretter en ny applikation er det tanken at denne Program_App_Plus_SendEmail_template.cs kopieres til din app Program_App_Plus_SendEmail.cs (Og sættes til Compile)
/// Når du opgraderer NuGet Pakken "dk.gi.app.console.template.GiNugetSrc", så gentages kopieringen herover, men nu med overskriv (Har du rettet, så red dine rettelser først)
/// </summary>
using System;

// GI

//namespace dk.gi.app.console.template
namespace dk.gi.app.laan.csv.generator
{
    /// <summary>
    /// Koden her er den del af klassen GIConsoleApp (Partial)
    /// </summary>
    public partial class GIConsoleApp
    {

        /// <summary>
        /// Send en Email om status på kørsel af job
        /// 20220329 JMW Rettet så der nu bruges den E-mail webclient via Azure Graph
        /// </summary>
        private void SendEmail(int resultat)
        {
            try
            {
                if (appConfig.EmailModtagere.Length >= 1)
                {
                    // Indholdet af mail
                    string subject = $"Programfejl App:{appConfig.appName}, Miljø:{appConfig.CrmServerName}, Mode:{appConfig.Mode}, Dato:{System.DateTime.Now.ToString("yyyy-MM-dd hh:mm")}";
                    string strbody = $"Der opstod fejl:{resultat}, og programmet er ikke kørt til ende. Sporing skriver ikke til fil på app service";
                    // Opret context med email konfiguration som send mail med ovenstående indhold
                    dk.gi.email.EmailContext eContext = new dk.gi.email.EmailContext(appConfig.EmailClientId, appConfig.EmailClientSecret, appConfig.EmailTenantid, appConfig.EmailAfsenderMailAdressse);
                    if (dk.gi.email.EmailClient.SendEmail(eContext, subject, strbody, appConfig.EmailModtagere) == false)
                        Trace.LogError("SendEmail fejl fik ikke sendt E-mail");
                }
            }
            catch (Exception ex)
            {
                Trace.LogError("SendEmail fejl: " + ex.Message.ToString());
            }
        }
    }
}