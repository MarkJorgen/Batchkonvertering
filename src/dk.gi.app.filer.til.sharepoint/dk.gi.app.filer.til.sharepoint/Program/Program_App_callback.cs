using dk.gi.crm;
using dk.gi.crm.data.bll;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Extensions.Logging;
using System;

//namespace dk.gi.app.console.template
namespace dk.gi.app.filer.til.sharepoint
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
        private AppStatus CallBackFunction()
        {
            AppStatus appStatus = new AppStatus();

            Trace.LogInformation("CallBackFunction blev kaldt");

            FilerTilSharePointRequest filerTilSharePointRequest = new FilerTilSharePointRequest(this.crmcontext)
            {
                CrmConnectionString = appConfig.GetCrmConnectionString,
                EmailClientId = appConfig.EmailClientId,
                EmailClientSecret = appConfig.EmailClientSecret,
                EmailAfsenderMailAdressse = appConfig.EmailAfsenderMailAdressse,
                EmailTenantid = appConfig.EmailTenantid,
                CrmServerName = appConfig.CrmServerName,
                EmailModtagere = appConfig.EmailModtagere
            };
            CrmResponse filerTilSharePointResult = filerTilSharePointRequest.Execute<CrmResponse>();

            if (!filerTilSharePointResult.Success)
            {
                if (filerTilSharePointResult.UserErrorMessage != string.Empty)
                {
                    appStatus.SetStatusTekstmsg = filerTilSharePointResult.UserErrorMessage;
                }
                else
                {
                    appStatus.SetStatusTekstmsg = filerTilSharePointResult.Status.Message;
                }
                appStatus.SetStatus = AppStatus.StateCode.AppExceptionInCode;
            }

            Trace.LogInformation($"CallBackFunction slut {appStatus}");

            return appStatus;
        }
    }
}