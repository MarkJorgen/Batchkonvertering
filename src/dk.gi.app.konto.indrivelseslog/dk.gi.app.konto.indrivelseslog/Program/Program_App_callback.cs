using dk.gi.crm;
using dk.gi.crm.data.bll;
using dk.gi.crm.managers.V2;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Extensions.Logging;
using System;

//namespace dk.gi.app.console.template
namespace dk.gi.app.konto.indrivelseslog
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

            DateTime datoSletFor = new DateTime(DateTime.Today.Year - 6, 12, 31, 23, 59, 59).ToLocalTimeGI();

            Trace.LogInformation($"Sletter oprettede inddrivelseslogs før/lig med {datoSletFor.ToLongDateString() + " " + datoSletFor.ToLongTimeString()}");

            try
            {
                using (Ap_inddrivelseslogManager inddrivelseslogManager = new Ap_inddrivelseslogManager(crmcontext))
                {
                    inddrivelseslogManager.SletAlleUdenKontoRef();
                }

                int antal = 1;

                while (antal > 0)
                    antal = Slet(datoSletFor, 100);

            }
            catch (Exception ex)
            {
                Trace.LogError($"Fejl i CallBackFunction {ex.Message} {ex.StackTrace}");
                result = AppStatus.StateCode.AppUventetFejlIProgramKode;
            }

            Trace.LogInformation($"CallBackFunction slut {result}");

            return result;
        }


        internal int Slet(DateTime datoSletFor, int sletAntal)
        {
            Trace.LogInformation($"Sletter antal: {sletAntal}");

            CrmContext _crmcontext = new dk.gi.crm.CrmContext(appConfig.GetCrmConnectionString);

            using (Ap_inddrivelseslogManager inddrivelseslogManager = new Ap_inddrivelseslogManager(_crmcontext))
            {
                return inddrivelseslogManager.SletAntalPaaDatoEllerFoer(datoSletFor, 100);
            }
        }

    }
}