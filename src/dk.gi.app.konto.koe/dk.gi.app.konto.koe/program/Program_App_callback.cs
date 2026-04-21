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
namespace dk.gi.app.konto.koe
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

            // ****************************************
            // FlytKoeSlettedeKonti     Flytter opgørelser for slettede konti, til afsluttet-køen, såfremt opgørelserne ikke allerede er i afsluttet-køen.
            // Konti     Valgfri.Angiver en eller flere specifikke konti, som jobbet skal køres for. Det enkelt kontonummer, kan angives med eller            
            //              uden bindestreg(41 - 12345 eller 4112345).Skal der angives flere konti, så adskil kontonumre med et komma(41 - 00001, 41 - 00002).
            // ****************************************
            #region  Flytter opgørelser for slettede konti, til afsluttet-køen
            FlytTilAfsluttetKoeForSlettedeKontiRequest req1 = new FlytTilAfsluttetKoeForSlettedeKontiRequest(crmcontext);
            if (appConfig.ContainsKey("konti") == true)
                req1.KontoNumre = appConfig["konti"].Split(',');
            FlytTilAfsluttetKoeForSlettedeKontiResponse resp1 = req1.Execute<FlytTilAfsluttetKoeForSlettedeKontiResponse>();
            if (resp1.Status.IsOK() == false)
                result = AppStatus.StateCode.AppUventetFejlIProgramKode; // Din fejlkode som retuneres til OS -  lad den stå medmindre du ønsker et andet tal (bruges nedenfor til fejlhåndtering)
            #endregion
 
            // ****************************************
            // TilfoejKoe     Tilføjelse af kø, til alle opgørelser, der ikke allerede har en kø.
            // Konti     Valgfri.Angiver en eller flere specifikke konti, som jobbet skal køres for. Det enkelt kontonummer, kan angives med eller            
            //              uden bindestreg(41-12345 eller 4112345).Skal der angives flere konti, så adskil kontonumre med et komma(41-00001,41-00002).
            // ****************************************
            #region  Tilføjelse af kø, til alle opgørelser
            TilfoejKoeRequest req2 = new TilfoejKoeRequest(crmcontext);
            if (appConfig.ContainsKey("konti") == true)
                req2.KontoNumre = appConfig["konti"].Split(',');
            TilfoejKoeResponse resp2 = req2.Execute<TilfoejKoeResponse>();
            if (resp2.Status.IsOK() == false)
                result = AppStatus.StateCode.AppUventetFejlIProgramKode; // Din fejlkode som retuneres til OS -  lad den stå medmindre du ønsker et andet tal (bruges nedenfor til fejlhåndtering)
            #endregion
         
            // ****************************************
            // FlytKoeOpgoerelse     Flytter opgørelser placeret i opgørelses-køen (kode = 000001), til enten rykker- eller afsluttet-køen.
            // Konti     Valgfri.Angiver en eller flere specifikke konti, som jobbet skal køres for. Det enkelt kontonummer, kan angives med eller            
            //              uden bindestreg(41 - 12345 eller 4112345).Skal der angives flere konti, så adskil kontonumre med et komma(41 - 00001, 41 - 00002).
            // ****************************************
            #region Flytter opgørelser
            FlytFraOpgoerelsekoeRequest req3 = new FlytFraOpgoerelsekoeRequest(crmcontext);
            req3.doNotSetAP_Opkrvningafsendt = true;  // Vi ønsker ikke denne sat her i Batch
            if (appConfig.ContainsKey("konti") == true)
                req3.KontoNumre = appConfig["konti"].Split(',');

            FlytFraOpgoerelsekoeResponse resp3 = req3.Execute<FlytFraOpgoerelsekoeResponse>();
            if (resp3.Status.IsOK() == false)
                result = AppStatus.StateCode.AppUventetFejlIProgramKode; // Din fejlkode som retuneres til OS -  lad den stå medmindre du ønsker et andet tal (bruges nedenfor til fejlhåndtering)
            #endregion

            // ****************************************
            // FlytKoeRykker     Flytter opgørelser placeret i rykker-køen (kode = 000002), til enten inddrivelses- eller afsluttet-køen.
            // konti     Valgfri.Angiver en eller flere specifikke konti, som jobbet skal køres for. Det enkelt kontonummer, kan angives med eller
            //              uden bindestreg(41 - 12345 eller 4112345).Skal der angives flere konti, så adskil kontonumre med et komma(41 - 00001, 41 - 00002).
            // ****************************************
            #region Flytter rykker
            FlytFraRykkerkoeRequest req4 = new FlytFraRykkerkoeRequest(crmcontext);
            if (appConfig.ContainsKey("konti") == true)
                req4.KontoNumre = appConfig["konti"].Split(',');
            FlytFraRykkerkoeResponse resp4 = req4.Execute<FlytFraRykkerkoeResponse>();
            if (resp4.Status.IsOK() == false)
                result = AppStatus.StateCode.AppUventetFejlIProgramKode; // Din fejlkode som retuneres til OS -  lad den stå medmindre du ønsker et andet tal (bruges nedenfor til fejlhåndtering)
            #endregion
            //}
            // ****************************************
            // FlytKoeInddrivelse     Flytter opgørelser placeret i inddrivelses-køen (kode = 000003), til afsluttet-køen.
            //fo - konti     Valgfri.Angiver en eller flere specifikke konti, som jobbet skal køres for. Det enkelt kontonummer, kan angives med eller            
            //              uden bindestreg(41 - 12345 eller 4112345).Skal der angives flere konti, så adskil kontonumre med et komma(41 - 00001, 41 - 00002).
            // -MODE=FlytKoeInddrivelse
            // ****************************************
            //if (appConfig.Mode.ToUpper() == "FLYTKOEINDDRIVELSE")
            //{
            #region Flytter inddrivelser
            FlytFraInddrivelseskoeRequest req5 = new FlytFraInddrivelseskoeRequest(crmcontext);
            if (appConfig.ContainsKey("konti") == true)
                req4.KontoNumre = appConfig["konti"].Split(',');
            FlytFraInddrivelseskoeResponse resp5 = req5.Execute<FlytFraInddrivelseskoeResponse>();
            if (resp5.Status.IsOK() == false)
                result = AppStatus.StateCode.AppUventetFejlIProgramKode; // Din fejlkode som retuneres til OS -  lad den stå medmindre du ønsker et andet tal (bruges nedenfor til fejlhåndtering)
            #endregion
            //}

            Trace.LogInformation("CallBackFunction slut");
            return result;
        }
    }
}