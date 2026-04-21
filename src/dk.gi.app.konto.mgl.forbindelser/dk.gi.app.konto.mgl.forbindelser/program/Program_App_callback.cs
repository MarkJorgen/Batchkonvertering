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
using dk.gi.crm;
using dk.gi.crm.data;

//namespace dk.gi.app.console.template
namespace dk.gi.app.konto.mgl.forbindelser
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

            ForbindelseBLL forbindelseBLL = new ForbindelseBLL(crmcontext);
            CrmResponse response = forbindelseBLL.TilfoejForbindelserTilSager();

            if (response.Status.IsOK() == false)
            {
                result = AppStatus.StateCode.AppUventetFejlIProgramKode; // Din fejlkode som retuneres til OS -  lad den stå medmindre du ønsker et andet tal (bruges nedenfor til fejlhåndtering)
            }

            Trace.LogInformation("CallBackFunction slut");
            return result;
        }
    }
}