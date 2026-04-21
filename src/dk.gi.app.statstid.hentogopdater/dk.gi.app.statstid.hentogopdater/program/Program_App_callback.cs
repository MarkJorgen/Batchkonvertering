/// <summary>
/// Koden her er den del af klassen GIConsoleApp (Partial)
/// 
/// Version: 2022 12 19
/// Sidste ændring: Changed Result pattern to be AppStatus and not AppStatus.StateCode
///
/// Det er primært i denne at du skal rette, her skal den primære aktuelle program kode lægges
/// </summary>

using dk.gi;
using dk.gi.cpr.servicelink;
using dk.gi.crm.data.Statstidende;
using dk.gi.crm.giproxy;
using dk.gi.crm.managers.V2;
using dk.gi.crm.statstid.hentogopdater;
using dk.gi.email;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using System;
using System.Configuration;

//namespace dk.gi.app.console.template
namespace dk.gi.crm.app.statstid.hentogopdater
{
    /// <summary>
    /// Din kode skal tilføjes i Metoden Start her i dette objekt
    /// </summary>
    public partial class GIConsoleApp
    {
        AppStatus appStatus = new AppStatus();

        /// <summary>
        /// Her udføres den egentlige behandling
        /// </summary>
        /// <returns>AppStatus</returns>
        private void CallBackFunction()
        {
            Trace.LogInformation("CallBackFunction blev kaldt");

            //// ****************************************
            //// Indtast tekst her
            //// -MODE=XXXXXX Hvis mode er xxxxx så kaldes request xyz som henter/opdaterer/sletter eller .....
            //// ****************************************
            //if (appConfig.Mode.ToUpper() == "BATCH")
            //{

            ModeBatch();

            //}

            Trace.LogInformation("CallBackFunction slut");
        }

        private AppStatus ModeBatch()
        {
            AppStatus result = new AppStatus();

            Integrationslog integrationslog = new Integrationslog();

            Guid integrationslogId = integrationslog.Opret(crmcontext, new OptionSetValue((int)ap_integrationslog_ap_status.Igang), new OptionSetValue((int)ap_integrationslog_ap_dataleverandoer.Statstidende),
                new OptionSetValue((int)ap_integrationslog_ap_integrationspartner.Statstidende));

            KontoOpdaterMedStatstidendeRequest kontoOpdaterMedStatstidendeRequest = new KontoOpdaterMedStatstidendeRequest(crmcontext)
            {
                Mode = "batch",
                KundeId = appConfig["kundeId"],
                BrugerKonto = appConfig["brugerKonto"],
                MailKonto = appConfig["mailKonto"],
                MailTvangsauktion = appConfig["mailTvangsauktion"],
                SendEmails = System.Convert.ToBoolean(crmcontext.GetConfigSettingSingle("app.statstid.hentogopdater.konto.send.emails")),
                SagsTypeKonkurs = Guid.Parse(appConfig["sagsTypeKonkurs"]),
                SagsEmneKonkurs = appConfig["sagsEmneKonkurs"],
                OpgaveKodeKonkurs = appConfig["opgaveKodeKonkurs"],
                OpgaveEmneKonkurs = appConfig["opgaveEmneKonkurs"],
                SagsTypeEjerskifte = Guid.Parse(appConfig["sagsTypeEjerskifte"]),
                SagsEmneEjerskifte = appConfig["sagsEmneEjerskifte"],
                OpgaveKodeEjerskifte = appConfig["opgaveKodeEjerskifte"],
                OpgaveEmneEjerskifte = appConfig["opgaveEmneEjerskifte"],
                SagsTypeTvangsauktion = Guid.Parse(appConfig["sagsTypeTvangsauktion"]),
                SagsEmneTvangsauktion = appConfig["sagsEmneTvangsauktion"],
                OpgaveKodeTvangsauktion = appConfig["opgaveKodeTvangsauktion"],
                OpgaveEmneTvangsauktion = appConfig["opgaveEmneTvangsauktion"],
                EmailClientId = appConfig.EmailClientId,
                EmailClientSecret = appConfig.EmailClientSecret,
                EmailTenantid = appConfig.EmailTenantid,
                EmailAfsenderMailAdressse = appConfig.EmailAfsenderMailAdressse,
                VentFoerNaesteKald = 1000 * int.Parse(appConfig["ventFoerNaesteKald"]),
                MailTvangsauktionAabentVedligehold = crmcontext.GetConfigSettingSingle("app.statstid.hentogopdater.laan.email.adresse")
            };
            GenericStringResponse kontoOpdaterMedStatstidendeResponse = kontoOpdaterMedStatstidendeRequest.Execute<GenericStringResponse>();

            if (kontoOpdaterMedStatstidendeResponse.Status.IsErrorOrWarning())
            {
                result.SetStatus = AppStatus.StateCode.AppUventetFejlIProgramKode;
                result.SetStatusTekstmsg = kontoOpdaterMedStatstidendeResponse.Status.Message;    
                
                try
                {
                    integrationslog.OpdaterStatus(crmcontext, integrationslogId, new OptionSetValue((int)ap_integrationslog_ap_status.FejletCRM));
                    throw new Exception(kontoOpdaterMedStatstidendeResponse.Status.Message);
                }
                catch
                {
                    throw new Exception(kontoOpdaterMedStatstidendeResponse.Status.Message);
                }
            }
            else
            {
                // Alt gik godt
                integrationslog.OpdaterStatus(crmcontext, integrationslogId, new OptionSetValue((int)ap_integrationslog_ap_status.AfsluttetOKCRM));
            }

            return result;
        }

    }
}