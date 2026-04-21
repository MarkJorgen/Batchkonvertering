using dk.gi.crm;
using dk.gi.crm.data.bll;
using dk.gi.crm.giproxy;
using dk.gi.crm.managers.V2;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dk.gi.app.konto.satser.opret.Application
{
    internal sealed class OpretSatserWorkflow
    {
        public OpretSatserWorkflowResult Execute(CrmContext crmcontext, ILogger logger, int satsAar)
        {
            var result = new OpretSatserWorkflowResult
            {
                SatsAar = satsAar
            };

            logger.LogInformation($"Der skal oprettes satser for år {satsAar}");

            try
            {
                using (var kontoManager = new Ap_KontoManager(crmcontext))
                using (var reguleringsprocentManager = new AP_reguleringsprocentManager(crmcontext))
                using (var satserManager = new Ap_SatserManager(crmcontext))
                {
                    List<AP_konto> konti = kontoManager
                        .HentAlleStatusAktivEllerBero(
                            AP_konto.Fields.Id,
                            AP_konto.Fields.AP_Kontonr,
                            AP_konto.Fields.AP_Bindingstype,
                            AP_konto.Fields.ap_lovgrundlag)
                        .Where(k =>
                            k.AP_Bindingstype.GetValueOrDefault() == false &&
                            k.ap_lovgrundlag.GetValueOrDefault() == (int)AP_konto_ap_lovgrundlag.Førjuli2015)
                        .OrderBy(k => k.AP_Kontonr)
                        .ToList();

                    result.CandidateCount = konti.Count;

                    List<AP_reguleringsprocent> reguleringsprocenter = reguleringsprocentManager
                        .HentAlle()
                        .Where(rp => rp.AP_Startdato.Value.ToLocalTimeGI().Year == satsAar)
                        .ToList();

                    if (reguleringsprocenter.Count == 0)
                    {
                        result.StatusCode = AppStatus.StateCode.AppExceptionInCode;
                        result.ErrorMessage = $"Reguleringsprocent for {satsAar} blev ikke fundet!";
                        return result;
                    }

                    if (reguleringsprocenter.Count > 1)
                    {
                        result.StatusCode = AppStatus.StateCode.AppExceptionInCode;
                        result.ErrorMessage = $"Reguleringsprocent for {satsAar} blev fundet flere gange!";
                        return result;
                    }

                    if (satserManager.ErSatserOprettetTilAar(satsAar))
                    {
                        result.StatusCode = AppStatus.StateCode.AppExceptionInCode;
                        result.ErrorMessage = $"Satser for {satsAar} er allerede oprettet!";
                        return result;
                    }

                    var reguleringsprocent = reguleringsprocenter.First();
                    var beregnSatsRequest = new BeregnSatsRequest(crmcontext);

                    foreach (AP_konto konto in konti)
                    {
                        logger.LogInformation($"Opretter sats for {konto.AP_Kontonr}");

                        beregnSatsRequest.BatchNr = Guid.NewGuid().ToString().Substring(0, 20);
                        beregnSatsRequest.Beregningslog = false;
                        beregnSatsRequest.Reguleringsprocent = reguleringsprocent;
                        beregnSatsRequest.ReguleringsprocentId = reguleringsprocent.AP_reguleringsprocentId.Value;
                        beregnSatsRequest.Genberegning = false;
                        beregnSatsRequest.Konto = konto;
                        beregnSatsRequest.KontoNr = konto.AP_Kontonr;

                        BeregnSatsResponse beregnSatsResponse = beregnSatsRequest.Execute<BeregnSatsResponse>();

                        if (!beregnSatsResponse.Status.IsOK())
                        {
                            result.StatusCode = AppStatus.StateCode.AppExceptionInCode;
                            result.ErrorMessage = "beregnSatsRequest fejlede eller timede ud - dk.gi.app.konto.satser.opret skal køres igen - tjek log";
                            return result;
                        }

                        result.CreatedCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Der opstod en uventet fejl i OpretSatserWorkflow");
                result.StatusCode = AppStatus.StateCode.AppExceptionInCode;
                result.ErrorMessage = $"Der opstod en uventet fejl! : {ex.Message}";
                return result;
            }

            return result;
        }
    }

    internal sealed class OpretSatserWorkflowResult
    {
        public AppStatus.StateCode StatusCode { get; set; } = AppStatus.StateCode.OK;
        public string ErrorMessage { get; set; } = string.Empty;
        public int SatsAar { get; set; }
        public int CandidateCount { get; set; }
        public int CreatedCount { get; set; }
    }
}
