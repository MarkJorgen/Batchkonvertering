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
using dk.gi.crm.response;
using dk.gi.crm.request.V2;
using dk.gi.crm.giproxy;
using dk.gi.crm.response.V2;
using dk.gi.crm.models;
using dk.gi.crm.data;
using System.Drawing;
using System.IdentityModel.Metadata;
using System.Threading;
using System.Linq;

//namespace dk.gi.app.console.template
namespace dk.gi.crm.app.konto.kontrol.rykbilag
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
            result.SetStatus = AppStatus.StateCode.OK;
            Trace.LogInformation("CallBackFunction blev kaldt");


            if (appConfig.ContainsAll(new string[] { "KundeId", "Bruger", "OpgaveAktivitetKode", "OpgaveAktivitetEmne", "OpgaveAktivitetBeskrivelse", "DokumentNavn" }) == true)
            {
                Trace.LogInformation("Create Request: HentDataFraBilagSkalRykkesKoeRequest");
                bool tilladSendTilDigitalPost = bool.Parse(crmcontext.GetConfigSettingSingle("App.konto.kontrol.rykbilag.TilladSendTilDigitalPost"));
                HentDataFraBilagSkalRykkesKoeRequest hentDataFraBilagSkalRykkesKoeRequest = new HentDataFraBilagSkalRykkesKoeRequest(crmcontext)
                {
                    KundeId = appConfig["KundeId"],
                    BrugerNavn = appConfig["Bruger"],
                    BegraensetDataLoad = false,
                    ValiderBruger = false,
                    StikproeveStatusCode = AP_stikprve_StatusCode.Bilagskalrykkes1gangviabatch,
                    LukAktiviteterMedEmnekoder = new string[] { appConfig["OpgaveAktivitetKode"] }
                };
                Trace.LogInformation("Get result from: HentDataFraBilagSkalRykkesKoeRequest");
                HentDataFraBilagSkalRykkesKoeResponse hentDataFraBilagSkalRykkesKoeResponse = hentDataFraBilagSkalRykkesKoeRequest.Execute<HentDataFraBilagSkalRykkesKoeResponse>();
                if (hentDataFraBilagSkalRykkesKoeResponse.Status.IsOK() != true)
                {
                    result.SetStatus = AppStatus.StateCode.AppUventetFejlIProgramKode;
                    return result; // 
                }

                Trace.LogInformation($"HentDataFraBilagSkalRykkesKoeRequest result:{hentDataFraBilagSkalRykkesKoeResponse.Kontroller.Count()}");
                foreach (BilagRykkesKoeItemModel bilagRykkesKoeItemModel in hentDataFraBilagSkalRykkesKoeResponse.Kontroller)
                {
                    Trace.LogInformation($"Sener job til Azure Kontonr:{bilagRykkesKoeItemModel.KontoNr}, kontrolid:{bilagRykkesKoeItemModel.KontrolId}");
                    CreateJobKontrolRykBilag(tilladSendTilDigitalPost, bilagRykkesKoeItemModel);
                }

            }
            else
                result.SetStatus = AppStatus.StateCode.AppRequiredParmsMissing;

            Trace.LogInformation("CallBackFunction slut");
            return result;
        }

        /// <summary>
        /// Create a job i Azure Service Bus Queue
        /// </summary>
        /// <param name="tilladSendTilDigitalPost"></param>
        /// <param name="bilagRykkesKoeItemModel"></param>
        /// <returns></returns>
        private bool CreateJobKontrolRykBilag(bool tilladSendTilDigitalPost, BilagRykkesKoeItemModel bilagRykkesKoeItemModel)
        {
            Trace.LogInformation($"{GetType().Name}.KontrolRykBilag start");
            bool result = true;

            // 2022 07 27 RCL Er OpretOpgoerelse false skal vi lige tjekke inddrivelselog
            if (bilagRykkesKoeItemModel != null)
            {
                dk.gi.asbq.JsonKeyValueList jsonbody = new dk.gi.asbq.JsonKeyValueList();
                jsonbody.AddKeyValue("TilladSendTilDigitalPost", tilladSendTilDigitalPost == true ? "true" : "false");
                jsonbody.AddKeyValue("OpgaveAktivitetKode", appConfig["OpgaveAktivitetKode"]);
                jsonbody.AddKeyValue("OpgaveAktivitetEmne", appConfig["OpgaveAktivitetEmne"]);
                jsonbody.AddKeyValue("OpgaveAktivitetBeskrivelse", appConfig["OpgaveAktivitetBeskrivelse"]);
                jsonbody.AddKeyValue("DokumentNavn", appConfig["DokumentNavn"]);
                jsonbody.AddKeyValue("KontoNr", bilagRykkesKoeItemModel.KontoNr);
                jsonbody.AddKeyValue("Sagsnummer", bilagRykkesKoeItemModel.Sagsnummer);
                jsonbody.AddKeyValue("KontrolId", bilagRykkesKoeItemModel.KontrolId.ToStringForCRM());
                jsonbody.AddKeyValue("IndkaldtAar", bilagRykkesKoeItemModel.RegnskabDatoTil.Value.Year.ToString());
                jsonbody.AddKeyValue("Indkaldtdato", bilagRykkesKoeItemModel.KontrolBilagRekvireretDato.Value.ToLongDateString());
                jsonbody.AddKeyValue("udgifter", bilagRykkesKoeItemModel.KontrolOprindeligUdgift.GetValueOrDefault().ToString("n2"));
                jsonbody.AddKeyValue("LukAktivitetMedEmnekode", appConfig["OpgaveAktivitetKode"]);
                jsonbody.AddKeyValue("Svarfrist", bilagRykkesKoeItemModel.Svarfrist.Value.ToLongDateString());
                jsonbody.AddKeyValue("DAGSDATO", DateTime.Today.ToLongDateString());

                // Azure Service Bus Queue
                dk.gi.asbq.jobqueue koe = new dk.gi.asbq.jobqueue(crmcontext);
                // Config setting:Azure.Service.Bus.Queue.Label.KONTROLRYKBILAG
                result = koe.SendToQueue(jsonbody, dk.gi.asbq.AzureServiceBusQueueLabels.KONTROLRYKBILAG);
            }

            Trace.LogInformation($"{GetType().Name}.KontrolRykBilag slut");
            return result;
        }
    }
}