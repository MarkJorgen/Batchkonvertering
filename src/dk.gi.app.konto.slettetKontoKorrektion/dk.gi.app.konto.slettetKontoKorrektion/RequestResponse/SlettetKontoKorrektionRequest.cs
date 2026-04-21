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
using dk.gi.crm.response;
using Microsoft.Extensions.Logging;
using dk.gi.crm.managers.V2;
using dk.gi.crm.managers.specialized;
using Microsoft.Xrm.Sdk.Query;
using dk.gi.crm.giproxy;
using Microsoft.Xrm.Sdk;
using dk.gi.asbq;
//using dk.gi.crm.giproxy;
//using dk.gi.crm.managers.V2;

// Namespace til dit request objekt
namespace dk.gi.crm.app.konto.slettetKontoKorrektion
{
    /// <summary>
    /// A template for use in creating new requests (Copy this file to a new file)
    /// 
    /// Termplate inherits from IRequest, GIRequest or CrmRequest
    /// - You may/can not create a new request with a constructor without parameters, at lest a Trace objekt for tracking
    /// - please create a constructor without parameters and then set it to Obsolete to prevent unwanted use!
    /// </summary>
    public class SlettetKontoKorrektionRequest : CrmRequest // If this is a CRM request
    {
        public SlettetKontoKorrektionRequest(CrmContext context) : base(context) { }  // If this is a CRM request

        //[DataMember(IsRequired = true)]
        //public string yourProperty {get; set;} = value;

        /// <summary>
        /// Funktionen ExecuteRequest indeholder den kode der skal udføres.
        /// </summary>
        /// <returns>Et response som altid indeholder en Status på hvordan udførsel gik</returns>
        protected override IResponse ExecuteRequest()
        {
            SlettetKontoKorrektionResponse result = new SlettetKontoKorrektionResponse();

            if (result.Status.IsOK() == false)
                return result;

            // Validering is done
            Trace.LogInformation($"Validereing af egenskaber i request {GetType().Name}, blev fuldført uden fejl.");

            try
            {
                Trace.LogInformation($"Hent konti.");

                using (Ap_RegnskabsaarsagManager managerRegnskabsaarsager = new Ap_RegnskabsaarsagManager(this.localCrmContext))
                using (Ap_RegnskabManager managerRegnskab = new Ap_RegnskabManager(this.localCrmContext))
                using (Ap_KontoManager managerKonto = new Ap_KontoManager(this.localCrmContext))
                using (DynamicManager dynamicManager = new DynamicManager(this.localCrmContext))
                {
                    var query = new QueryExpression(AP_konto.EntityLogicalName);
                    query.ColumnSet.AllColumns = false;
                    string[] columns = { AP_konto.Fields.Id, AP_konto.Fields.AP_kontoId, AP_konto.Fields.AP_Kontonr, AP_konto.Fields.AP_18saldo, AP_konto.Fields.AP_bindingspligt, AP_konto.Fields.AP_Sidsteregnskabsdato };
                    query.ColumnSet.AddColumns(columns);
                    query.Criteria.AddCondition(AP_konto.Fields.AP_statusframapper, ConditionOperator.Equal, 2); // Slettet
                    query.Distinct = false;
                    query.NoLock = true;

                    var regnskabsaarsager = managerRegnskabsaarsager.Hent(new string[] { "20" }, AP_regnskabsrsager.Fields.Id, AP_regnskabsrsager.Fields.AP_Kode, AP_regnskabsrsager.Fields.AP_name).ToDictionary(k => k.AP_Kode, v => v);

                    //List<AP_konto> konti = managerKonto.HentAlleForQuery(query).ToList();
                    List<AP_konto> konti = dynamicManager.RetrieveAllByQueryExpression<AP_konto>(query).ToList();

                    string kontoNr = "";
                    int sec = 0;

                    foreach (AP_konto konto in konti)
                    {
                        AP_regnskab regnskaber = managerRegnskab.HentAlleRegnskaber(konto.Id, null, AP_regnskab.Fields.Id).FirstOrDefault();
                        try
                        {
                            if (regnskaber == null)
                            {
                                konto.AP_18saldo = new Money(0);
                                konto.AP_bindingspligt = new Money(0);
                                managerKonto.Update(konto);
                                Trace.LogInformation($"konto ({konto.AP_Kontonr}) managerkonto.Update OK!");
                            }
                            else
                            {
                                if ((konto.AP_18saldo != null && konto.AP_18saldo.Value != 0) ||
                                (konto.AP_bindingspligt != null && konto.AP_bindingspligt.Value != 0))
                                {
                                    kontoNr = konto.AP_Kontonr;
                                    AP_regnskab regnskab = new AP_regnskab();

                                    if (konto.AP_18saldo != null && konto.AP_18saldo.Value != 0)
                                    {
                                        regnskab.AP_Pgrf18henst = new Money(konto.AP_18saldo.Value * -1);
                                    }
                                    else
                                    {
                                        regnskab.AP_Pgrf18henst = new Money(Math.Max(decimal.Zero, 0));
                                    }

                                    if (konto.AP_bindingspligt != null && konto.AP_bindingspligt.Value != 0)
                                    {
                                        regnskab.AP_Prgf18bhenst = new Money(konto.AP_bindingspligt.Value * -1);
                                    }
                                    else
                                    {
                                        regnskab.AP_Prgf18bhenst = new Money(Math.Max(decimal.Zero, 0));
                                    }

                                    regnskab.ap_kontoid = konto.ToEntityReference();
                                    regnskab.ap_regnskabrsagid = regnskabsaarsager["20"].ToEntityReference();

                                    if (konto.AP_Bindingstype != null)
                                    {
                                        if (konto.AP_Bindingstype.Value)
                                        {
                                            //false 18b
                                            regnskab.AP_Bindingstype = new OptionSetValue(1);
                                        }
                                        else
                                        {
                                            //true 63a
                                            regnskab.AP_Bindingstype = new OptionSetValue(2);
                                        }
                                    }

                                    if (konto.AP_Sidsteregnskabsdato != null && konto.AP_Sidsteregnskabsdato.Value <= new DateTime(9999, 12, 30))
                                    {
                                        DateTime kontoSidsteRegnskabsdato = konto.AP_Sidsteregnskabsdato.Value.ToLocalTime();
                                        regnskab.AP_Periodestart = kontoSidsteRegnskabsdato.Date.AddDays(-1);
                                        regnskab.AP_Periodeslut = kontoSidsteRegnskabsdato.Date;
                                        managerRegnskab.Create(regnskab);
                                        Trace.LogInformation($"konto ({konto.AP_Kontonr}) managerRegnskab.Create OK!");

                                        InsertJobToMessageQueue(konto.AP_kontoId.Value, sec);
                                        sec += 15;
                                    }
                                    else
                                    {
                                        konto.AP_18saldo = new Money(0);
                                        konto.AP_bindingspligt = new Money(0);
                                        managerKonto.Update(konto);
                                        Trace.LogInformation($"konto ({konto.AP_Kontonr}) managerkonto.Update OK!");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log fejlen
                            Trace.LogError($"Kunne indsætte regnskab for konto ({kontoNr})" + ex.ToString());
                        }
                    }
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


        private void InsertJobToMessageQueue(Guid ap_kontoid, int sec)
        {
            Trace.LogInformation("InsertJobToMessageQueue start");

            dk.gi.asbq.JsonKeyValueList model = new dk.gi.asbq.JsonKeyValueList();
            JsonKeyValueList jsonkeyvaluelist = new JsonKeyValueList();
            jsonkeyvaluelist.AddKeyValue("Mode", "Regnskab");
            jsonkeyvaluelist.AddKeyValue("action", "reberegn");
            jsonkeyvaluelist.AddKeyValue("ap_konto", ap_kontoid.ToStringForCRM());
            jsonkeyvaluelist.AddKeyValue("Kilde", "SlettetKontoKorrektionRequest");

            dk.gi.asbq.jobqueue koe = new dk.gi.asbq.jobqueue(this.localCrmContext);
            if (koe.SendToQueue(jsonkeyvaluelist, asbq.AzureServiceBusQueueLabels.Kontodiv, 0, 0, 0, sec) == false)
            {
                throw new Exception($"Azure Job regnskab reberegn Kontodiv blev ikke oprettet!");
            }

            Trace.LogInformation("InsertJobToMessageQueue slut");
        }
    }
}