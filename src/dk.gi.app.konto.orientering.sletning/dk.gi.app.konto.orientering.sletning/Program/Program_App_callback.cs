using dk.gi.crm;
using dk.gi.crm.data.bll;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Extensions.Logging;
using System;

//namespace dk.gi.app.console.template
namespace dk.gi.app.konto.orientering.sletning
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


            bool tilladSendTilDigitalPost = false;

            Trace.LogInformation("Henter værdi for App.konto.orientering.sletning.TilladSendTilDigitalPost configuration settings");

            if (crmcontext.GetConfigSettingSingle("App.konto.orientering.sletning.TilladSendTilDigitalPost") == "true")
            {
                tilladSendTilDigitalPost = true;
            }

            DateTime datoOprettet = new DateTime(DateTime.Today.Year - 3, 1, 1);

            KontoOrienteringSletning kontoOrienteringSletning = new KontoOrienteringSletning(crmcontext, Trace, tilladSendTilDigitalPost, datoOprettet, appConfig["KundeId"], appConfig["BrugerNavn"],
                    appConfig["SagsEmne"], Guid.Parse(appConfig["SagsType"]), appConfig["OpgaveAktivitetskode"], appConfig["OpgaveAktivitetEmne"], int.Parse(appConfig["SvarFrist"]));

            if (kontoOrienteringSletning.Afvikling() == false)
                result = AppStatus.StateCode.AppExceptionInCode;

            Trace.LogInformation($"CallBackFunction slut {result}");

            return result;
        }
    }
}