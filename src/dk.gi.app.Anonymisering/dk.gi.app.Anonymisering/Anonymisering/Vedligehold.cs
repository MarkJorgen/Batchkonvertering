// History
// YYYY MM DD INIT Description
// =====================================================================================================
// 2018 08 20 JMW  JMW Oprettet på baggrund af sag 5038137
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
    internal static class Vedligehold
    {
        internal static void Anonymisering(CrmContext context, string afleveretRigsarkivet, ref AppStatus result)
        {
            context.Trace.LogInformation($"Vedligehold.anonymisering start {afleveretRigsarkivet}");

            try
            {
                DateTime? afleveretRigsarkivetDato = null;

                if (!string.IsNullOrEmpty(afleveretRigsarkivet))
                {
                    //afleveretRigsarkivetDato = System.Convert.ToDateTime(afleveretRigsarkivet);

                    string[] split = afleveretRigsarkivet.Split('-');
                    afleveretRigsarkivetDato = new DateTime(int.Parse(split[2]), int.Parse(split[1]), int.Parse(split[0]));
                }

                context.Trace.LogInformation($"Dato afleveretRigsarkivetDato : {afleveretRigsarkivetDato.Value.ToLongDateString()}");

                // Opret søgning som finder alle vedligehold som opfylder kriterier
                FindOgAnonymiserVedligeholdRequest req = new FindOgAnonymiserVedligeholdRequest(context)
                {
                    AntalAarTilAnonymiseringVedligehold = 5,
                    AntalAarTilAnonymiseringLaanIndfriet = 5,
                    AfleveretRigsarkivet = afleveretRigsarkivetDato
                };
                // Udfør query og få et resultat tilbage
                FindOgAnonymiserVedligeholdResponse resp = req.Execute<FindOgAnonymiserVedligeholdResponse>();
                if (resp.Status.IsOK() == false)
                {
                    context.Trace.LogError("Der opstod en fejl undervejs i anonymisering af Vedligehold.");
                    result.SetStatus = AppStatus.StateCode.AppExceptionInCode;
                }
            }
            catch (Exception ex)
            {
                result.SetStatus = AppStatus.StateCode.AppExceptionInCode;
                context.Trace.LogError(ex, null, null);
                throw;
            }

            context.Trace.LogInformation("Vedligehold.anonymisering Slut");
        }
    }
}
