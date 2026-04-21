// Indhold i angivne felter skal fjernes når dato for bortfald er historisk 
// - ap_name tømmes for indhold, men sættes til at være postnr fra den ejendom sager er relateret til.
// - ap_ejendomid tømmes
// - ap_kontaktpersonid tømmes

//
// History
// YYYY MM DD INIT Description
// =====================================================================================================
// 2019 04 29 ANJ  Oprettet iforbindelse med opgavebeskrivelse fra Henning
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
    internal static class LaanUdenEjendom
    {
        internal static void Anonymisering(CrmContext context, ref AppStatus result, string modeType)
        {
            context.Trace.LogInformation("LaanUdenEjendom.anonymisering start");
            try
            {
                // Opret søgning som finder alle vedligehold som opfylder kriterier
                FindOgAnonymiserLaanUdenEjendomRequest req = new FindOgAnonymiserLaanUdenEjendomRequest(context)
                {
                    ModeType = modeType
                };
                // Udfør query og få et resultat af de behandlede sager retur
                FindOgAnonymiserLaanUdenEjendomResponse resp = req.Execute<FindOgAnonymiserLaanUdenEjendomResponse>();
                if (resp.Status.IsOK() == false)
                {
                    context.Trace.LogError("Der opstod en fejl undervejs i Anonymisering af lån den ejendom.");
                    result.SetStatus = AppStatus.StateCode.AppExceptionInCode;
                }


                switch (modeType)
                {
                    case "TEST":
                        foreach (var laan in resp.data)
                        {
                            Console.WriteLine($"AP_laanId: {laan.AP_laanId} AP_laanenr: {laan.AP_laanenr}");
                        }

                        Console.WriteLine($"Count: {resp.data.Count}");
                        Console.ReadKey();
                        break;

                    case "DEAKTIVERING":
                        foreach (var laan in resp.data)
                        {
                            Console.WriteLine($"AP_laanId: {laan.AP_laanId} AP_laanenr: {laan.AP_laanenr}");
                        }

                        Console.WriteLine($"Count: {resp.data.Count}");
                        Console.ReadKey();
                        break;


                    case "SLETNING":
                        Console.WriteLine($"IKKE IMPLEMENTERET ENDNU");
                        Console.ReadKey();
                        break;

                }

            }
            catch (Exception ex)
            {
                result.SetStatus = AppStatus.StateCode.AppExceptionInCode;
                context.Trace.LogError(ex, null, null);
                throw;
            }

            context.Trace.LogInformation("LaanUdenEjendoms.anonymisering Slut");
        }
    }
}
