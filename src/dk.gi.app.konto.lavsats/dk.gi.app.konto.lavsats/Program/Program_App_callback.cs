using dk.gi.crm.data.bll;
using Microsoft.Extensions.Logging;

//namespace dk.gi.app.console.template
namespace dk.gi.crm.app.konto.lavsats
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

            KontoLavSats kontoLavSats = new KontoLavSats(this.crmcontext);

            CrmResponse crmResult = kontoLavSats.Find(0);

            if (crmResult.Status.IsOK() == false)
            {
                result = AppStatus.StateCode.AppExceptionInCode;
            }

            Trace.LogInformation($"CallBackFunction slut {result}");

            return result; 
        }
    }
}