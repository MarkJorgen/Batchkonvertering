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
using dk.gi.crm.request.V2;
using System.IO;
using dk.gi.crm.response.V2;
using System.Linq;
using dk.gi.crm.giproxy;
using dk.gi.crm.models;

//namespace dk.gi.app.console.template
namespace dk.gi.crm.laan.app
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

            result.SetStatus = AppStatus.StateCode.OK; 

            // ****************************************
            // Kør den del der behandler opdatering af aktive sager
            // ****************************************
            if (appConfig.Mode.ToUpper() == "PROCESSUDBETALTE")
            {
                ProcessUdbetalteRequest udb = new ProcessUdbetalteRequest(crmcontext);
                if (appConfig.ContainsKey("SAGSNR") == true)
                    udb.laanNr = appConfig["SAGSNR"];
                CrmResponse res = udb.Execute();
                if (res.Status.IsErrorOrWarning())
                    result.SetStatus = AppStatus.StateCode.AppUventetFejlIProgramKode;
            }
            // ****************************************
            // Kør den del der sætter de indfriede lån til Anonymiserede
            // ****************************************
            //if (appConfig.Mode.ToUpper() == "PROCESSINDFRIEDE")
            //{
            //    ProcessIndfriede2AnonymizedRequest indf = new ProcessIndfriede2AnonymizedRequest(crmcontext);
            //    if (appConfig.ContainsKey("SAGSNR") == true)
            //        indf.sagsnr = appConfig["SAGSNR"];
            //    CrmResponse res = indf.Execute();
            //    if (res.Status.IsErrorOrWarning())
            //        result.SetStatus = AppStatus.StateCode.AppUventetFejlIProgramKode;
            //}
            // ****************************************
            // Kør den del der sætter de indfriede lån til Anonymiserede
            // ****************************************
            if (appConfig.Mode.ToUpper() == "PROCESSKASSEREDE")
            {
                ProcessAnonymiseredeRequest kass = new ProcessAnonymiseredeRequest(crmcontext);
                if (appConfig.ContainsKey("SAGSNR") == true)
                    kass.sagsnr = appConfig["SAGSNR"];
                CrmResponse res = kass.Execute();
                if (res.Status.IsErrorOrWarning())
                    result.SetStatus = AppStatus.StateCode.AppUventetFejlIProgramKode;
            }

            // ****************************************
            // Kør den del der sætter de indfriede lån til Anonymiserede
            // -MODE=setloankasseretFraIndfrietFromFile -File=laanenrliste.csv -username=csu@gisb.dk -password=Password-1 -domain=gisb -crmOrganisation=GI -crmServer=crm.udv.gi.dk
            // ****************************************
            if (appConfig.Mode.ToUpper() == "SETLOANKASSERETFRAINDFRIETFROMFILE")
            {
                if (appConfig.ContainsKey("FILE") == false)
                {
                    Trace.LogError("CSV Filnavn ikke angivet i parameter FILE=<filnavn>");
                }
                else
                {
                    foreach (string line in File.ReadLines(appConfig["FILE"]))
                    {
                        crmcontext.Trace.LogInformation("Behandler:" + line);
                        HentLaanRequest req = new dk.gi.crm.request.V2.HentLaanRequest(crmcontext)
                        {
                            SearchLaan = new SearchLaanModel()
                            {
                                searchLaanenummer = line
                            }
                        };
                        HentLaanResponse resp = req.Execute<HentLaanResponse>();
                        if (resp.Status.IsOK() == true)
                        {
                            if (resp.laanItems.First<LaanoversigItem>().laanStatus.HasValue == true)
                            {
                                if (resp.laanItems.First<LaanoversigItem>().laanStatus == (int)AP_laan_AP_Status.Indfriet)
                                {
                                    crmcontext.Trace.LogInformation($"Sæt status til kasseret lån:{line}");
                                    SetLaanStatusAdminRequest statReq = new SetLaanStatusAdminRequest(crmcontext)
                                    {
                                        LaanId = resp.laanItems.First<LaanoversigItem>().LaanId.Value,
                                        nyStatusPaaLaan = EnumLaanStatus.Kasseretefter5år
                                    };
                                    SetLaanStatusResponse status = statReq.Execute<SetLaanStatusResponse>();
                                    crmcontext.Trace.LogInformation($"Status lån:{line} Status:{status.Status.Message}");
                                }
                                else
                                    crmcontext.Trace.LogError($"Fejl Lån:{line} har ikke status Indfriet");
                            }
                            else
                                crmcontext.Trace.LogError($"Fejl Lån:{line} status:Ukendt");
                        }
                        else
                            result.SetStatus = AppStatus.StateCode.AppUventetFejlIProgramKode;
                    }

                }
            }

            // ****************************************
            // Kør den del der sætter de indfriede lån til Anonymiserede
            // -MODE=setloankasseretfraindfafslbortfromfile -File=laanenrliste.csv -username=csu@gisb.dk -password=Password-1 -domain=gisb -crmOrganisation=GI -crmServer=crm.udv.gi.dk
            // ****************************************
            if (appConfig.Mode.ToUpper() == "SETLOANKASSERETFRAINDFAFSLBORTFROMFILE")
            {
                if (appConfig.ContainsKey("FILE") == false)
                {
                    Trace.LogError("CSV Filnavn ikke angivet i parameter FILE=<filnavn>");
                }
                else
                {
                    foreach (string line in File.ReadLines(appConfig["FILE"]))
                    {
                        crmcontext.Trace.LogInformation("Behandler:" + line);
                        HentLaanRequest req = new dk.gi.crm.request.V2.HentLaanRequest(crmcontext)
                        {
                            SearchLaan = new SearchLaanModel()
                            {
                                searchLaanenummer = line
                            }
                        };
                        HentLaanResponse resp = req.Execute<HentLaanResponse>();
                        if (resp.Status.IsOK() == true)
                        {
                            if (resp.laanItems.First<LaanoversigItem>().laanStatus.HasValue == true)
                            {
                                if (resp.laanItems.First<LaanoversigItem>().laanStatus == (int)AP_laan_AP_Status.Indfriet
                                || resp.laanItems.First<LaanoversigItem>().laanStatus == (int)AP_laan_AP_Status.Afslag
                                || resp.laanItems.First<LaanoversigItem>().laanStatus == (int)AP_laan_AP_Status.Bortfaldet)
                                {
                                    crmcontext.Trace.LogInformation($"Sæt status til kasseret lån:{line}");
                                    SetLaanStatusAdminRequest statReq = new SetLaanStatusAdminRequest(crmcontext)
                                    {
                                        LaanId = resp.laanItems.First<LaanoversigItem>().LaanId.Value,
                                        nyStatusPaaLaan = EnumLaanStatus.Kasseretefter5år
                                    };
                                    SetLaanStatusResponse status = statReq.Execute<SetLaanStatusResponse>();
                                    crmcontext.Trace.LogInformation($"Status lån:{line} Status:{status.Status.Message}");
                                }
                                else
                                    crmcontext.Trace.LogError($"Fejl Lån:{line} har ikke status Indfriet,Afslag eller Bortfaldet");
                            }
                            else
                                crmcontext.Trace.LogError($"Fejl Lån:{line} status:Ukendt");
                        }
                        else
                            result.SetStatus = AppStatus.StateCode.AppUventetFejlIProgramKode;
                    }
                }
            }


            Trace.LogInformation("CallBackFunction slut");
            return result;
        }
    }
}