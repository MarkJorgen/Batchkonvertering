using System;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;

// CRM SDK NameSpace
// Microsoft CRM SDK
//using Microsoft.Xrm.Sdk;
//using Microsoft.Xrm.Sdk.Query;
//using Microsoft.Crm.Sdk.Messages;
//using Microsoft.Xrm.Sdk.Messages;

// GI namespace
using dk.gi;
using dk.gi.crm;
using dk.gi.crm.managers;
using dk.gi.crm.request;
using dk.gi.crm.response;
using Microsoft.Extensions.Logging;
using dk.gi.crm.response.V2;
using System.Data;
using dk.gi.crm.request.V2;
using dk.gi.crm.models;
using dk.gi.crm.managers.V2;
using System.Configuration;
using dk.gi.crm.giproxy;
using dk.gi.asbq;

namespace dk.gi.app.konto.opgoerelse.rykker
{
    /// <summary>
    /// DanKontoRykkerRequest
    /// </summary>
    public class DanKontoRykkerRequest : CrmRequest
    {
        public DanKontoRykkerRequest(CrmContext context) : base(context) { }

        /// <summary>
        /// Funktionen ExecuteRequest indeholder den kode der skal udføres.
        /// </summary>
        /// <returns>Et response som altid indeholder en Status på hvordan udførsel gik</returns>
        protected override IResponse ExecuteRequest()
        {
            DanKontoRykkerResponse result = new DanKontoRykkerResponse();

            if (result.Status.IsOK() == false)
                return result;

            // Validering is done
            Trace.LogInformation($"Validereing af egenskaber i request {GetType().Name}, blev fuldført uden fejl.");
            try
            {
                using (Ap_KontoSystemManager kontoSystemManager = new Ap_KontoSystemManager(this.localCrmContext))
                using (Ap_KontoManager kontoManager = new Ap_KontoManager(this.localCrmContext))
                using (IncidentManager incidentManager = new IncidentManager(this.localCrmContext))
                using (IncidentManager incidentLaesManager = new IncidentManager(this.localCrmContext))
                using (ConnectionManager connectionManager = new ConnectionManager(this.localCrmContext))
                using (KontrolManager kontrolManager = new KontrolManager(this.localCrmContext))
                {
                    string indbetalingsDato = DateTime.Today.AddDays(kontoSystemManager.Vaerdier().RykkerFristDage).ToLongDateString();

                    #region Rykker 1
                    List<HentOpgoerelserFraKoeOpgoerelse> opgoerelserRykker1 =
                        OpgoerelserRykker1(kontoManager, incidentLaesManager, connectionManager, kontrolManager);

                    foreach (HentOpgoerelserFraKoeOpgoerelse opgoerelse in opgoerelserRykker1)
                    {
                        this.Trace.LogInformation("Opret JsonKeyValueList til job");
                        dk.gi.asbq.JsonKeyValueList model = new dk.gi.asbq.JsonKeyValueList();
                        model.AddKeyValue("action", "RYKKER1");
                        model.AddKeyValue("id", opgoerelse.Id.ToStringForCRM());
                        this.Trace.LogInformation("Opret job kø objekt");
                        dk.gi.asbq.jobqueue koe = new dk.gi.asbq.jobqueue(this.localCrmContext);
                        this.Trace.LogInformation("Send job til kø objekt");
                        koe.SendToQueue(model, AzureServiceBusQueueLabels.OpgoerelseRykker);
                    }
                    #endregion

                    #region Rykker 2
                    List<HentOpgoerelserFraKoeOpgoerelse> opgoerelserRykker2 = OpgoerelserRykker2(kontoSystemManager.Vaerdier().YderligereFristForFlytningTilNyKoeDage, 
                        kontoManager, incidentLaesManager, connectionManager, kontrolManager);

                    foreach (HentOpgoerelserFraKoeOpgoerelse opgoerelse in opgoerelserRykker2)
                    {
                        this.Trace.LogInformation("Opret JsonKeyValueList til job");
                        dk.gi.asbq.JsonKeyValueList model = new dk.gi.asbq.JsonKeyValueList();
                        model.AddKeyValue("action", "RYKKER2");
                        model.AddKeyValue("id", opgoerelse.Id.ToStringForCRM());
                        this.Trace.LogInformation("Opret job kø objekt");
                        dk.gi.asbq.jobqueue koe = new dk.gi.asbq.jobqueue(this.localCrmContext);
                        this.Trace.LogInformation("Send job til kø objekt");
                        koe.SendToQueue(model, AzureServiceBusQueueLabels.OpgoerelseRykker);
                    }
                    #endregion
                }

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

        /// <summary>
        /// OpgoerelserRykker1 opgoerelse.Rykkerdato is null
        /// </summary>
        /// <param name="kontoManager"></param>
        /// <param name="incidentManager"></param>
        /// <param name="connectionManager"></param>
        /// <param name="kontrolManager"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal List<HentOpgoerelserFraKoeOpgoerelse> OpgoerelserRykker1(Ap_KontoManager kontoManager, IncidentManager incidentManager,
            ConnectionManager connectionManager, KontrolManager kontrolManager)
        {
            Trace.LogInformation("OpgoerelserRykker1 start");

            List<HentOpgoerelserFraKoeOpgoerelse> opgoerelser = new List<HentOpgoerelserFraKoeOpgoerelse>();

            HentOpgoerelserFraKoeRequest hentOpgoerelserFraKoeRequest = new HentOpgoerelserFraKoeRequest(base.localCrmContext)
            {
                KoeKode = "000002"
            };
            HentOpgoerelserFraKoeResponse hentOpgoerelserFraKoeResponse = hentOpgoerelserFraKoeRequest.Execute<HentOpgoerelserFraKoeResponse>();

            if (hentOpgoerelserFraKoeResponse.Status.IsOK())
            {
                foreach (HentOpgoerelserFraKoeOpgoerelse opgoerelse in hentOpgoerelserFraKoeResponse.Opgoerelser)
                {
                    bool open = this.HarAabneSager(opgoerelse.Konto.Id, kontoManager, incidentManager, connectionManager, kontrolManager);

                    if (opgoerelse.Rykkerafsendt.HasValue == false && open == false)
                    {
                        opgoerelser.Add(opgoerelse);
                    }
                }

                Trace.LogInformation("OpgoerelserRykker1 slut");

                return opgoerelser;
            }
            else
            {
                throw new Exception(hentOpgoerelserFraKoeResponse.Status.Message);
            }
        }

        /// <summary>
        /// OpgoerelserRykker2 opgoerelse.Rykkerdato2 not null and Forfaldsdato + yderligereFristForFlytningTilNyKoeDage less then today
        /// </summary>
        /// <param name="yderligereFristForFlytningTilNyKoeDage"></param>
        /// <param name="kontoManager"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal List<HentOpgoerelserFraKoeOpgoerelse> OpgoerelserRykker2(int yderligereFristForFlytningTilNyKoeDage, Ap_KontoManager kontoManager,
            IncidentManager incidentManager, ConnectionManager connectionManager, KontrolManager kontrolManager)
        {
            Trace.LogInformation("OpgoerelserRykker2 start");

            List<HentOpgoerelserFraKoeOpgoerelse> opgoerelser = new List<HentOpgoerelserFraKoeOpgoerelse>();

            HentOpgoerelserFraKoeRequest hentOpgoerelserFraKoeRequest = new HentOpgoerelserFraKoeRequest(base.localCrmContext)
            {
                KoeKode = "000002"
            };
            HentOpgoerelserFraKoeResponse hentOpgoerelserFraKoeResponse = hentOpgoerelserFraKoeRequest.Execute<HentOpgoerelserFraKoeResponse>();

            if (hentOpgoerelserFraKoeResponse.Status.IsOK())
            {
                foreach (HentOpgoerelserFraKoeOpgoerelse opgoerelse in hentOpgoerelserFraKoeResponse.Opgoerelser)
                {
                    bool open = this.HarAabneSager(opgoerelse.Konto.Id, kontoManager, incidentManager, connectionManager, kontrolManager);

                    if (open == false)
                    {
                        if (opgoerelse.Rykkerafsendt.HasValue == true &&
                            opgoerelse.Rykker2afsendt.HasValue == false &&
                            opgoerelse.Rykkerdato.HasValue == true &&
                            opgoerelse.Rykkerdato.Value.Date.AddDays(yderligereFristForFlytningTilNyKoeDage).Date < DateTime.Today)
                        {
                            opgoerelser.Add(opgoerelse);
                        }
                    }
                }

                Trace.LogInformation("OpgoerelserRykker2 slut");

                return opgoerelser;
            }
            else
            {
                throw new Exception(hentOpgoerelserFraKoeResponse.Status.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kontoId"></param>
        /// <param name="kontoManager"></param>
        /// <param name="incidentManager"></param>
        /// <param name="connectionManager"></param>
        /// <param name="kontrolManager"></param>
        /// <returns></returns>
        internal bool HarAabneSager(Guid kontoId, Ap_KontoManager kontoManager, IncidentManager incidentManager,
            ConnectionManager connectionManager, KontrolManager kontrolManager)
        {
            bool open = kontoManager.HarAabneSager(kontoId, ekskluderSagerDerTilladerSelvbetjening: false);

            if (open)
            {
                Guid subjectKontrolId = Guid.Parse("dbca8e42-eaf8-e611-944c-0050568472ba");

                List<Incident> incidentsNotSubjectKontrol = incidentManager.HentSagerTilEntitet(kontoId, IncidentState.Active,
                    Incident.Fields.Id, Incident.Fields.IncidentId, Incident.Fields.SubjectId).ToList();

                List<Incident> incidentsNoSubject = incidentsNotSubjectKontrol.Where(i => i.SubjectId == null || i.SubjectId.Id == Guid.Empty).ToList();

                if (incidentsNoSubject.Count > 0)
                {
                    // We have open incidents without subject, so we consider it open  
                    Trace.LogInformation($"Vi fandt en åben sag uden subject som, så ikke kan være en kontrol. KontoId {kontoId}");
                    return true;
                }

                incidentsNotSubjectKontrol = incidentsNotSubjectKontrol.Where(i => i.SubjectId.Id != subjectKontrolId).ToList();

                if (incidentsNotSubjectKontrol.Count == 0)
                {
                    Trace.LogInformation($"OpgoerelserRykker1 ingen åbne sager forskellig fra kontrol. Tjek kontroller.");

                    List<Incident> incidentsSubjectKontrol = incidentManager.HentSagerTilEntitet(kontoId, IncidentState.Active,
                        Incident.Fields.Id, Incident.Fields.IncidentId, Incident.Fields.SubjectId).
                        Where(i => i.SubjectId.Id == subjectKontrolId).ToList();

                    if (incidentsSubjectKontrol.Count > 0)
                    {
                        Trace.LogInformation($"Vi fandt kontroller som tjekkes antal: {incidentsSubjectKontrol.Count}.");

                        open = false;

                        List<Connection> kontrolConnections = connectionManager.HentSagerKontrol(incidentsSubjectKontrol);

                        foreach (Connection kontrolConnection in kontrolConnections)
                        {
                            AP_stikprve kontrol = kontrolManager.Hent(kontrolConnection.Record2Id.Id, AP_stikprve.Fields.StatusCode);
                            if (kontrol.StatusCode.Value == (int)AP_stikprve_StatusCode.Kontroligang)
                            {
                                Trace.LogInformation($"Vi fandt en kontrol med status: kontrol i gang. Så vi har en åben sag.");
                                open = true;
                                break;
                            }
                        }
                    }
                }
            }

            return open;
        }
    }
}