//Frister:
// En tre-klip sag kan anonymiseres når de tilhørende registeringer er anonymiserede.
// - Dog ikke, hvis tre_klip feltet ap_status = DOM 
//   - i så fald bevares data indtil videre. (Yderligere aftales med MAM/THM)
//
// ap_ejerid
// ap_bemaerkningnaevn
// ap_bemaerkninganklagemyndighed
// ap_bemaerkningdom
// ap_bemaerkningafslutning
// ap_bemaerkning
//
// evt.noter skal slettes
//
// History
// YYYY MM DD INIT Description
// =====================================================================================================
// 2018 08 20 JMW  JMW Oprettet på baggrund af sag 5038137
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dk.gi.app;
//
using dk.gi.crm;
using dk.gi.crm.giproxy;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Extensions.Logging;

namespace dk.gi.crm.app.anonymisering
{
    internal static class TreKlip
    {
        internal static void Anonymisering(CrmContext context, ref AppStatus result)
        {
            context.Trace.LogInformation("TreKlip.anonymisering start");
            try
            {
                // Opret søgning som finder alle vedligehold som opfylder kriterier
                FindOgAnonymiserTreKlipRequest req = new FindOgAnonymiserTreKlipRequest(context)
                {
                     datoforAfsluttetErFoerDen = System.DateTime.Now.AddDays(-1).AddYears(-3)
                };
                // Udfør query og få et resultat af de behandlede sager retur
                FindOgAnonymiserTreKlipResponse resp = req.Execute<FindOgAnonymiserTreKlipResponse>();
                if (resp.Status.IsOK() == false)
                { 
                    context.Trace.LogError("Der opstod en fejl undervejs i Anonymisering af TreKlip.");
                    result.SetStatus = AppStatus.StateCode.AppExceptionInCode;
                }
            }
            catch (Exception ex)
            {
                result.SetStatus = AppStatus.StateCode.AppExceptionInCode;
                context.Trace.LogError(ex, null, null);
                throw;
            }

            context.Trace.LogInformation("TreKlip.anonymisering Slut");
        }
    }
}
