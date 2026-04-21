// Indhold i disse felter skal fjernes når dato for bortfald er historisk 
// - d.v.s dagen efter dato for bortfald(ap_datoforbortfald)
//
// ap_huslejenaevnetsjournalnummer
// ap_paabudtvedligeholdid
// ap_bemaerkning
// ap_ejendomid
// ap_ejendommensadresse
// ap_ejendommensmatrikelnr
// ap_ejendommensejer
// ap_treklipid
// ap_naevnskendelse
// ap_endeligkendelse
// ap_arbejdetigangsat
// ap_meddelelsetilnaevn
// ap_yderligereoplysningertilnaevn
// ap_dom
// ap_bbrnummer
//
// History
// YYYY MM DD INIT Description
// =====================================================================================================
// 2018 08 20 JMW  JMW Oprettet på baggrund af sag 5038137
//                     - Jeg undrer mig over at der er mange records uden data i ap_datoforbortfald
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
    internal static class Registrering
    {
        internal static void Anonymisering(CrmContext context, ref AppStatus result)
        {
            context.Trace.LogInformation("Registrering.anonymisering start");
            try
            {
                // Opret søgning som finder alle vedligehold som opfylder kriterier (dagen efter dato for bortfald)
                FindOgAnonymiserRegistreringRequest req = new FindOgAnonymiserRegistreringRequest(context)
                {
                    datoforbortfaldErFørDen  = System.DateTime.Now.AddDays(-1)
                };
                // Udfør query og få et resultat af de behandlede sager retur
                FindOgAnonymiserRegistreringResponse resp = req.Execute<FindOgAnonymiserRegistreringResponse>();
                if (resp.Status.IsOK() == false)
                {
                    context.Trace.LogError("Der opstod en fejl undervejs i Anonymisering af Registreringer.");
                    result.SetStatus = AppStatus.StateCode.AppExceptionInCode;
                }
            }
            catch (Exception ex)
            {
                result.SetStatus = AppStatus.StateCode.AppExceptionInCode;
                context.Trace.LogError(ex, null, null);
                throw;
            }

            context.Trace.LogInformation("Registrering.anonymisering Slut");
        }
    }
}
