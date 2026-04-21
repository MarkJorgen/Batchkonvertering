using System;

using Microsoft.Extensions.Logging;
// GI Using
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using System.Collections.Generic;
using System.Globalization;
using dk.gi.crm.models;
using dk.gi.crm.managers.V2;
using Microsoft.Xrm.Sdk;
using System.Linq;
using dk.gi.crm.giproxy;
using System.Windows;

//namespace dk.gi.app.console.template
namespace dk.gi.app.konto.regnskab.konverter
{
    /// <summary>
    /// Din kode skal tilføjes i Metoden Start her i dette objekt
    /// </summary>
    public partial class GIConsoleApp
    {
        /// <summary>
        /// Her udføres den egentlige behandling
        /// </summary>
        /// <returns>AppStatus.StateCode</returns>
        private AppStatus.StateCode CallBackFunction()
        {
            AppStatus.StateCode result = AppStatus.StateCode.OK;
            Trace.LogInformation("CallBackFunction blev kaldt");

            #region Konvertering fra 'Dannet regnskab' til 'Aflagt regnskab'.
            DateTime? dato = null;

            Trace.LogInformation($"Henter alle konti uden et dannet regnskab (0-regnskab)...");
           
            KonverteringTilAflagtRegnskabRequest req = new KonverteringTilAflagtRegnskabRequest(crmcontext);
            //req.doNotSetAP_Opkrvningafsendt = true;  // Vi ønsker ikke denne sat her i Batch
            //if (appConfig.ContainsKey("konti") == true)
            //    req.KontoNr = appConfig["konti"].Split(',').First();
            //if (appConfig.ContainsKey("SlutDato") == true)
            //    dato = DateTime.Parse(appConfig["SlutDato"]);
            req.Dato = dato;
            req.antalRetry = 3;
            req.antalKontiRetry = 15;

            KonverteringTilAflagtRegnskabResponse resp = req.Execute<KonverteringTilAflagtRegnskabResponse>();
            if (resp.Status.IsOK() == false)
            {
                result = AppStatus.StateCode.AppUventetFejlIProgramKode; // Din fejlkode som retuneres til OS -  lad den stå medmindre du ønsker et andet tal (bruges nedenfor til fejlhåndtering)
            }
            #endregion

            Trace.LogInformation("CallBackFunction slut");
            return result;
        }
    }
}