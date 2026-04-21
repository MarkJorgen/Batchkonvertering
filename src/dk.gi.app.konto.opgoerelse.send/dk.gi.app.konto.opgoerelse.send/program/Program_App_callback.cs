/// <summary>
/// Koden her er den del af klassen GIConsoleApp (Partial)
/// 
/// Version: 2022 12 19
/// Sidste ændring: Changed Result pattern to be AppStatus and not AppStatus.StateCode
///
/// Det er primært i denne at du skal rette, her skal den primære aktuelle program kode lægges
/// </summary>

using System;
using Microsoft.Extensions.Logging;
using dk.gi;
using dk.gi.crm.models;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using dk.gi.crm;
using dk.gi.crm.managers.V2;
using dk.gi.crm.giproxy;
using System.Collections.Generic;
using System.Linq;
using dk.gi.asm.crm;

//namespace dk.gi.app.console.template
namespace dk.gi.app.konto.opgoerelse.send
{
    /// <summary>
    /// Din kode skal tilføjes i Metoden Start her i dette objekt
    /// </summary>
    public partial class GIConsoleApp
    {
        /// <summary>
        /// Her udføres den egentlige behandling
        /// </summary>
        /// <returns>AppStatus</returns>
        private AppStatus CallBackFunction()
        {
            AppStatus result = new AppStatus();
            Trace.LogInformation("CallBackFunction blev kaldt");

            result = SaetKontiIKoe();

            Trace.LogInformation("CallBackFunction slut");
            return result;
        }

        private AppStatus SaetKontiIKoe()
        {
            AppStatus result = new AppStatus();
            result.SetStatus = AppStatus.StateCode.OK;

            this.Trace.LogInformation($"OpgoerelserFindSendRequest starter");

            OpgoerelserFindSendRequest opgoerelserFindSendRequest = new OpgoerelserFindSendRequest(this.crmcontext)
            {
            };
            GenericStringResponse opgoerelserFindSendResult = opgoerelserFindSendRequest.Execute<GenericStringResponse>();

            if (opgoerelserFindSendResult.Status.IsOK() == false)
            {
                result.SetStatus = AppStatus.StateCode.AppExceptionInCode;
                this.Trace.LogError(opgoerelserFindSendResult.Status.Message);
            }
            else
            {
                this.Trace.LogInformation($"OpgoerelserFindSendRequest afsluttet uden fejl");
            }

            return result;
        }
    }
}