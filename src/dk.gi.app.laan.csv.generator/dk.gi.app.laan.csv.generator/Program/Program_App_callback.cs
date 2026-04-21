using dk.gi.crm.app.LaanCsvGenerator;
using dk.gi.crm.data.bll;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Extensions.Logging;
using System;
using System.Net.NetworkInformation;

//namespace dk.gi.app.console.template
namespace dk.gi.app.laan.csv.generator
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
            Trace.LogInformation("CallBackFunction blev kaldt");

            AppStatus.StateCode result = AppStatus.StateCode.OK;

            LaanCsvGeneratorRequest laanCsvGeneratorRequest = new LaanCsvGeneratorRequest(this.crmcontext)
            {
                EmailClientId = appConfig.EmailClientId,
                EmailClientSecret = appConfig.EmailClientSecret,
                EmailTenantid = appConfig.EmailTenantid,
                EmailAfsenderMailAdressse = appConfig.EmailAfsenderMailAdressse,
            };
            LaanCsvGeneratorResponse laanCsvGeneratorResponse = laanCsvGeneratorRequest.Execute<LaanCsvGeneratorResponse>();

            if (laanCsvGeneratorResponse.Status.IsOK() != true)
            {
                result = AppStatus.StateCode.AppExceptionInCode;
            }

            Trace.LogInformation("CallBackFunction slut");

            return result;
        }
    }
}