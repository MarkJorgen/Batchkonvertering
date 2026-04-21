using dk.gi.crm;
using dk.gi.crm.data.bll;
using dk.gi.crm.giproxy;
using dk.gi.crm.managers.V2;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;

//namespace dk.gi.app.console.template
namespace dk.gi.app.konto.satser.opret
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

            int satsAar = DateTime.Today.Year + 1;

            Trace.LogInformation($"Der skal oprettes satser for år {satsAar}");

            try
            {
                using (var kontoManager = new Ap_KontoManager(crmcontext))
                using (var reguleringsprocentManager = new AP_reguleringsprocentManager(crmcontext))
                using (var satserManager = new Ap_SatserManager(crmcontext))
                {
                    List<AP_konto> konti = kontoManager.HentAlleStatusAktivEllerBero(AP_konto.Fields.Id, AP_konto.Fields.AP_Kontonr, AP_konto.Fields.AP_Bindingstype, AP_konto.Fields.ap_lovgrundlag)
                        .Where(k => k.AP_Bindingstype.GetValueOrDefault() == false && k.ap_lovgrundlag.GetValueOrDefault() == (int)AP_konto_ap_lovgrundlag.Førjuli2015)
                        .OrderBy(k => k.AP_Kontonr).ToList();

                    List<AP_reguleringsprocent> reguleringsprocent = reguleringsprocentManager.HentAlle().Where(rp => rp.AP_Startdato.Value.ToLocalTimeGI().Year == satsAar).ToList();

                    if (reguleringsprocent.Count == 0)
                    {
                        result = AppStatus.StateCode.AppExceptionInCode;
                        emailMessage = $"Reguleringsprocent for {satsAar} blev ikke fundet!";
                        return result;
                    }

                    if (reguleringsprocent.Count > 1)
                    {
                        result = AppStatus.StateCode.AppExceptionInCode;
                        emailMessage = $"Reguleringsprocent for {satsAar} blev fundet flere gange!";
                        return result;
                    }

                    if (satserManager.ErSatserOprettetTilAar(satsAar))
                    {
                        result = AppStatus.StateCode.AppExceptionInCode;
                        emailMessage = $"Satser for {satsAar} er allerede oprettet!";
                        return result;
                    }

                    BeregnSatsRequest beregnSatsRequest = new BeregnSatsRequest(crmcontext);

                    foreach (AP_konto konto in konti)
                    {
                        Trace.LogInformation($"Opretter sats for {konto.AP_Kontonr}");

                        beregnSatsRequest.BatchNr = Guid.NewGuid().ToString().Substring(0, 20);
                        beregnSatsRequest.Beregningslog = false;
                        beregnSatsRequest.Reguleringsprocent = reguleringsprocent.First();
                        beregnSatsRequest.ReguleringsprocentId = reguleringsprocent.First().AP_reguleringsprocentId.Value;
                        beregnSatsRequest.Genberegning = false;
                        beregnSatsRequest.Konto = konto;
                        beregnSatsRequest.KontoNr = konto.AP_Kontonr;

                        BeregnSatsResponse beregnSatsResponse = beregnSatsRequest.Execute<BeregnSatsResponse>();

                        if (beregnSatsResponse.Status.IsOK() != true)
                        {
                            emailMessage = "beregnSatsRequest fejlede eller timede ud - dk.gi.app.konto.satser.opret skal køres igen - tjek log";
                            result = AppStatus.StateCode.AppExceptionInCode;
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.LogError(ex.Message);

                result = AppStatus.StateCode.AppExceptionInCode;
                emailMessage = $"Der opstod en uventet fejl! : {ex.Message}";
                return result;
            }

            Trace.LogInformation($"CallBackFunction slut {result}");

            return result;
        }
    }
}