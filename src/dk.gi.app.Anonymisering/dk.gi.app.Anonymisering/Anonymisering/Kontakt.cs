// Indhold i angivne felter skal fjernes når dato for bortfald er historisk 
// - ap_name tømmes for indhold, men sættes til at være postnr fra den ejendom sager er relateret til.
// - ap_ejendomid tømmes
// - ap_kontaktpersonid tømmes

//
// History
// YYYY MM DD INIT Description
// =====================================================================================================
// 2020 02 28 ANJ  Oprettet iforbindelse med anonymiserings opgave
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dk.gi.app;
using dk.gi.crm;
using dk.gi.crm.giproxy;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Extensions.Logging;

namespace dk.gi.crm.app.anonymisering
{
    internal static class Kontakt
    {
        internal static void Anonymisering(CrmContext context, ref AppStatus result)
        {
            context.Trace.LogInformation("Kontakt.anonymisering start");
            try
            {
                // Opret søgning som finder alle vedligehold som opfylder kriterier
                FindOgAnonymiserKontaktRequest req = new FindOgAnonymiserKontaktRequest(context) { };

                // Udfør query og få et resultat af de behandlede ejendomme retur
                FindOgAnonymiserKontaktResponse resp = req.Execute<FindOgAnonymiserKontaktResponse>();
                if (resp.Status.IsOK() == false)
                {
                    context.Trace.LogError("Der opstod en fejl undervejs i Anonymisering af Kontakt.");
                    result.SetStatus = AppStatus.StateCode.AppExceptionInCode;
                }
            }
            catch (Exception ex)
            {
                result.SetStatus = AppStatus.StateCode.AppExceptionInCode;
                context.Trace.LogError(ex, null, null);
                throw;
            }

            context.Trace.LogInformation("Kontakt.anonymisering Slut");
        }
    }
}
