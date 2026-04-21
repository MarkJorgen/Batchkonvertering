// Indhold i angivne felter skal fjernes når dato for bortfald er historisk 
// - ap_name tømmes for indhold, men sættes til at være postnr fra den ejendom sager er relateret til.
// - ap_ejendomid tømmes
// - ap_kontaktpersonid tømmes

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
    internal static class TeknikerService
    {
        internal static void Anonymisering(CrmContext context, ref AppStatus result, DateTime? ExplicitChangedOndate)
        {
            context.Trace.LogInformation("TeknikerService.anonymisering start");
            try
            {
                // Opret søgning som finder alle vedligehold som opfylder kriterier
                FindOgAnonymiserTeknikerServiceRequest req = new FindOgAnonymiserTeknikerServiceRequest(context)
                {
                    AntalAarUnChanged = 1
                };
                // Hack: Hvis der er specifik dato hvor changedon er sat
                if (ExplicitChangedOndate.HasValue == true)
                    req.ExplicitDate = ExplicitChangedOndate;

                // Udfør query og få et resultat af de behandlede sager retur
                FindOgAnonymiserTeknikerServiceResponse resp = req.Execute<FindOgAnonymiserTeknikerServiceResponse>();
                if (resp.Status.IsOK() == false)
                {
                    context.Trace.LogError("Der opstod en fejl undervejs i Anonymisering af TeknikerServiceer.");
                    result.SetStatus = AppStatus.StateCode.AppExceptionInCode;
                }
            }
            catch (Exception ex)
            {
                result.SetStatus = AppStatus.StateCode.AppExceptionInCode;
                context.Trace.LogError(ex, null, null);
                throw;
            }

            context.Trace.LogInformation("TeknikerService.anonymisering Slut");
        }
    }
}
