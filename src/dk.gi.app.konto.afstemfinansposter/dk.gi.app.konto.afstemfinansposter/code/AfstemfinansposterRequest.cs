using System;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;
// GI namespace
using dk.gi;
using dk.gi.crm;
using dk.gi.crm.managers;
using dk.gi.crm.request;
using dk.gi.crm.response;
using Microsoft.Extensions.Logging;
using dk.gi.crm.response.V2;
using dk.gi.crm.app.konto.afstemfinansposter;
using Microsoft.Xrm.Sdk;
using dk.gi.crm.giproxy;
using System.Configuration;
using dk.gi.crm.models;
using System.Diagnostics;
//using dk.gi.crm.giproxy;
//using dk.gi.crm.managers.V2;

// Namespace til dit request objekt
namespace dk.gi.crm.request.V2
{
    /// <summary>
    /// AfstemfinansposterRequest
    /// 
    /// </summary>
    public class AfstemfinansposterRequest : CrmRequest // If this is a CRM request
    {
        public AfstemfinansposterRequest(CrmContext context) : base(context) { }  // If this is a CRM request

        /// <summary>
        /// 
        /// </summary>
        [DataMember(IsRequired = true)]
        public string FinansAfstemning { get; set; }

        /// <summary>
        /// /
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Integrationslog_fra { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Integrationslog_til { get; set; }

        /// <summary>
        /// Log IntegrationsNavn
        /// </summary>
        [DataMember(IsRequired = true)]
        public string IntegrationsNavn { get; set; }

        /// <summary>
        /// Funktionen ExecuteRequest indeholder den kode der skal udføres.
        /// </summary>
        /// <returns>Et response som altid indeholder en Status på hvordan udførsel gik</returns>
        protected override IResponse ExecuteRequest()
        {
            // if needed change GenericStrignResponse to GenericGuidResponse or a response of your own choice
            AfstemfinansposterResponse result = new AfstemfinansposterResponse();

            if (result.Status.IsOK() == false)
                return result;

            // Validering is done
            Trace.LogInformation($"Validereing af egenskaber i request {GetType().Name}, blev fuldført uden fejl.");
            try
            {
                // Your code starts here
                Integrationslog integrationslog = null;
                Guid integrationslogId = Guid.Empty;
                #region Opret forbindelser

                // Vi starter integrationslog
                integrationslog = new Integrationslog();

                //string filnavn = DateTime.Now.Date.Year.ToString() + "-" + DateTime.Now.Date.Month.ToString() + "-" + DateTime.Now.Date.Day.ToString() + "-Finanspostering afstemning";
                integrationslogId = integrationslog.Opret(this.localCrmContext, new OptionSetValue((int)ap_integrationslog_ap_status.Igang), new OptionSetValue((int)ap_integrationslog_ap_dataleverandoer.GI),
                    new OptionSetValue((int)ap_integrationslog_ap_integrationspartner.Økonomi), this.IntegrationsNavn, "filnavn", "filnavn");
                // Vi gemmer id til status opdatering

                // FilManager filManager = new FilManager(Trace, this.FinansAfstemning);
                #endregion

                #region Initier ønskede fra/til afstemningsdatoer
                // DateTime sidstAfstemteDato = new DateTime(DateTime.Now.Year, 1, 1);
                // DateTime sidstAfstemteDato = filManager.SidstAfstemteDato().Date;
                DateTime sidstAfstemteDato = integrationslog.SidsteOKOpdateringsDato(this.localCrmContext);

                DateTime datoFra = sidstAfstemteDato.Date;
                DateTime datoTil = DateTime.Today.Date;

                Trace.LogInformation($"Fundne afstemningsdatoer dato fra {datoFra.Date} dato til {datoTil.Date}.");
                #endregion

                #region Foretag afstemning
                if (datoTil > datoFra)
                {
                    // Denne dato skal ikke afstemmes
                    datoFra = sidstAfstemteDato.Date.AddDays(1);

                    AfstemningSum afstemningSum = null;

                    KontoIndestaaendeBC kontoIndestaaendeBC = new KontoIndestaaendeBC(this.localCrmContext, datoFra, datoTil);

                    Trace.LogInformation($"Dato check til er større {datoTil}. Afstem finansposteringer.");

                    afstemningSum = kontoIndestaaendeBC.DatoForOkAfstemning();

                    if (afstemningSum.NyAfstemteDato > sidstAfstemteDato)
                    {
                        // filManager.GemNyAfstemteDato(afstemningSum.NyAfstemteDato);
                        sidstAfstemteDato = afstemningSum.NyAfstemteDato;
                    }

                    // 2020 09 15 RCL Opdater integrationsnavn med saldi 
                    integrationslog.OpdaterNavnMedSaldi(this.localCrmContext, integrationslogId, afstemningSum, this.IntegrationsNavn);

                    if (afstemningSum.NyAfstemteDato == datoTil)
                    {
                        Trace.LogInformation("Vi fandt ingen difference vi har gemt ny afstemt dato. dk.gi.crm.app.konto.afstemfinansposter kørsel afsluttet.");
                        integrationslog.OpdaterStatus(this.localCrmContext, integrationslogId, new OptionSetValue((int)ap_integrationslog_ap_status.AfsluttetOKCRM),
                            this.Integrationslog_til, this.Integrationslog_fra);
                    }
                    else
                    {
                        Trace.LogInformation("Der er differencer. Vi skal danne afstemningsdata.");
                        integrationslog.OpdaterStatus(this.localCrmContext, integrationslogId, new OptionSetValue((int)ap_integrationslog_ap_status.FejletCRM),
                            this.Integrationslog_til, this.Integrationslog_fra);

                        AfstemningData afstemningData = new AfstemningData();
                        afstemningData.AfstemningSum = afstemningSum;

                        AabneposterBC aabneposterBC = new AabneposterBC(this.localCrmContext, afstemningSum);
                        afstemningData.AabnePosteringer = aabneposterBC.Afstem();

                        // filManager.GemAfstemningData(afstemningData);
                    }
                }
                #endregion

                // Your code ends here

                // Information to trace, code completed this method without exceptions
                Trace.LogInformation($"Request {GetType().Name} blev gennemført");
            }
            catch (Exception ex)
            {
                result.Status.AppendError($"Der opstod en Exception i {GetType().Name}", ex);
            }

            // This is the output of the work done in this request
            return result;
        }
    }
}