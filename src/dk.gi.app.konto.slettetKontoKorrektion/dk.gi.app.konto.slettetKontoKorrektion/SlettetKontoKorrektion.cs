using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

// GI
using dk.gi.crm.giproxy;
using dk.gi.crm.managers;
using dk.gi.crm.managers.specialized;
using dk.gi.crm.managers.V2;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace dk.gi.crm.app.konto.slettetKontoKorrektion
{
    /// <summary>
    /// En forespørgsel til at spørge på NytRegnskabStatusMapperSlettetRequest
    /// </summary>
    /// <remarks>
    /// Oprettet af RCL, den 2018 03 01
    /// </remarks>
    public class SlettetKontoKorrektionRequest
    {

        #region properties
        #endregion

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public void ExecuteRequest(CrmContext context)
        {
            try
            {
                context.Trace.Information($"Hent konti.");

                using (Ap_RegnskabsaarsagManager managerRegnskabsaarsager = new Ap_RegnskabsaarsagManager(context))
                using (Ap_RegnskabManager managerRegnskab = new Ap_RegnskabManager(context))
                using (Ap_KontoManager managerKonto = new Ap_KontoManager(context))
                using (DynamicManager dynamicManager = new DynamicManager(context))
                {
                    var query = new QueryExpression(AP_konto.EntityLogicalName);
                    query.ColumnSet.AllColumns = false;
                    string[] columns = { AP_konto.Fields.Id, AP_konto.Fields.AP_Kontonr, AP_konto.Fields.AP_18saldo, AP_konto.Fields.AP_bindingspligt, AP_konto.Fields.AP_Sidsteregnskabsdato };
                    query.ColumnSet.AddColumns(columns);
                    query.Criteria.AddCondition(AP_konto.Fields.AP_statusframapper, ConditionOperator.Equal, 2); // Slettet
                    query.Distinct = false;
                    query.NoLock = true;

                    var regnskabsaarsager = managerRegnskabsaarsager.Hent(new string[] { "20" }, AP_regnskabsrsager.Fields.Id, AP_regnskabsrsager.Fields.AP_Kode, AP_regnskabsrsager.Fields.AP_name).ToDictionary(k => k.AP_Kode, v => v);

                    //List<AP_konto> konti = managerKonto.HentAlleForQuery(query).ToList();
                    List<AP_konto> konti = dynamicManager.RetrieveAllByQueryExpression<AP_konto>(query).ToList();

                    string kontoNr = "";

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
                                context.Trace.Information($"konto ({konto.AP_Kontonr}) managerkonto.Update OK!");
                            }
                            else
                            {
                                if ((konto.AP_18saldo != null && konto.AP_18saldo.Value != 0) ||
                                (konto.AP_bindingspligt != null && konto.AP_bindingspligt.Value != 0))
                                {
                                    kontoNr = konto.AP_Kontonr;
                                    AP_regnskab regnskab = new AP_regnskab();

                                    if (konto.AP_18saldo != null || konto.AP_18saldo.Value != 0)
                                    {
                                        regnskab.AP_Pgrf18henst = new Money(konto.AP_18saldo.Value * -1);
                                    }
                                    else
                                    {
                                        regnskab.AP_Pgrf18henst = new Money(Math.Max(decimal.Zero, 0));
                                    }

                                    if (konto.AP_bindingspligt != null || konto.AP_bindingspligt.Value != 0)
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
                                        context.Trace.Information($"konto ({konto.AP_Kontonr}) managerRegnskab.Create OK!");
                                    }
                                    else
                                    {
                                        konto.AP_18saldo = new Money(0);
                                        konto.AP_bindingspligt = new Money(0);
                                        managerKonto.Update(konto);
                                        context.Trace.Information($"konto ({konto.AP_Kontonr}) managerkonto.Update OK!");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log fejlen
                            context.Trace.Error($"Kunne indsætte regnskab for konto ({kontoNr})" + ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log fejlen
                context.Trace.Error(ex.ToString());
                throw ex;
            }
        }
    }
}