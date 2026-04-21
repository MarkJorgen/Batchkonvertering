using dk.gi.crm.data.bll;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Extensions.Logging;
using System;

//namespace dk.gi.app.console.template
namespace dk.gi.app.slet.udbetal.opgoer
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

            DateTime sletFoerDato = new DateTime(DateTime.Today.AddYears(-10).Year, 1, 1);

            Trace.LogInformation($"Sletter før SletFoerDato: {sletFoerDato.ToString("yyyy-MM-dd")}");

            try
            {
                SletUdbetalingerRequest sletUdbetalingerRequest = new SletUdbetalingerRequest(this.crmcontext)
                {
                    SletFoerDato = sletFoerDato
                };
                SletUdbetalingerResponse sletUdbetalingerResponse = sletUdbetalingerRequest.Execute<SletUdbetalingerResponse>();

                Trace.LogInformation($"SletUdbetalingerRequest fik status ok: {sletUdbetalingerResponse.Status.IsOK() != true}");

                if (sletUdbetalingerResponse.Status.IsOK() != true)
                {
                    emailMessage = "SletUdbetalingerRequest fejlede eller timede ud - dk.gi.app.slet.udbetal.opgoer skal køres igen - tjek log";
                    result = AppStatus.StateCode.AppExceptionInCode;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Trace.LogError(ex.Message);
                emailMessage = "SletUdbetalingerRequest fejlede eller timede ud - dk.gi.app.slet.udbetal.opgoer skal køres igen - tjek log";
                result = AppStatus.StateCode.AppExceptionInCode;
                return result;
            }

            try
            {
                SletOpgorelserRequest sletOpgorelserRequest = new SletOpgorelserRequest(this.crmcontext)
                {
                    SletFoerDato = sletFoerDato
                };
                SletOpgoerelserResponse sletOpgoerelserResponse = sletOpgorelserRequest.Execute<SletOpgoerelserResponse>();

                Trace.LogInformation($"SletOpgorelserRequest fik status ok: {sletOpgoerelserResponse.Status.IsOK() != true}");

                if (sletOpgoerelserResponse.Status.IsOK() != true)
                {
                    emailMessage = "SletOpgorelserRequest fejlede eller timede ud - dk.gi.app.slet.udbetal.opgoer skal køres igen - tjek log";
                    result = AppStatus.StateCode.AppExceptionInCode;
                    return result;
                }
            }
            catch (Exception ex)
            {
                Trace.LogError(ex.Message);
                emailMessage = "SletOpgorelserRequest fejlede eller timede ud - dk.gi.app.slet.udbetal.opgoer skal køres igen - tjek log";
                result = AppStatus.StateCode.AppExceptionInCode;
                return result;
            }

            Trace.LogInformation($"CallBackFunction slut {result}");

            return result;
        }
    }
}