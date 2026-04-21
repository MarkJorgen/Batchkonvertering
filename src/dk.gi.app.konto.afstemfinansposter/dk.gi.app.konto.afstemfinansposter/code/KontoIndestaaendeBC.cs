using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dk.gi.bc.servicelink;
using dk.gi.crm.models;
using dk.gi.crm.managers.V2;
using Microsoft.Extensions.Logging;

namespace dk.gi.crm.app.konto.afstemfinansposter
{
    public class KontoIndestaaendeBC
    {
        CrmContext CRMContext { get; set; }

        DateTime DatoFra { get; set; }

        DateTime DatoTil { get; set; }

        string KontoForIndestaaende { get; set; }

        public KontoIndestaaendeBC(CrmContext crmContext, DateTime datoFra, DateTime datoTil)
        {
            this.CRMContext = crmContext;
            this.DatoFra = datoFra;
            this.DatoTil = datoTil;
            Ap_KontoSystemManager managerKontoSystem = new Ap_KontoSystemManager(this.CRMContext);
            this.KontoForIndestaaende = managerKontoSystem.Vaerdier().KontoForIndestaaende;
        }

        /// <summary>
        /// Sammenligner konto indestående mellem crm og økonomi. Returnere SumAfstemning.
        /// </summary>
        public AfstemningSum DatoForOkAfstemning()
        {
            AfstemningSum afstemningSum = new AfstemningSum();

            Ap_KontoManager managerKonto = new Ap_KontoManager(this.CRMContext);

            decimal crmkontoindestaaende = managerKonto.HentIndestaaendeBeloeb() + managerKonto.HentTilbageholdtBeloeb();

            this.CRMContext.Trace.LogInformation($"crm kontoindestående er {crmkontoindestaaende}.");

            decimal oekonomiKontoindestaaende = HentIndestaaendeOekonomi(new DateTime(this.DatoTil.Year, 12, 31)) * -1;

            // Først prøver vi med sum/sum hvilket burde gå op når der ikke er fejl

            if (crmkontoindestaaende == oekonomiKontoindestaaende)
            {
                this.CRMContext.Trace.LogInformation($"Vi fandt overensstemmelse. Vi returner sidst afstemte ok dato til {this.DatoTil.Date}.");
                afstemningSum.NyAfstemteDato = this.DatoTil;
                afstemningSum.CRMDatoSaldo.Dato = this.DatoTil;
                afstemningSum.CRMDatoSaldo.Saldo = crmkontoindestaaende;
                afstemningSum.OekonomiDatoSaldo.Dato = this.DatoTil;
                afstemningSum.OekonomiDatoSaldo.Saldo = oekonomiKontoindestaaende;
                return afstemningSum;
            }

            // Det gik ikke så vi finder det bedste match
            return DatoForMatchAfstemning(crmkontoindestaaende, oekonomiKontoindestaaende);
        }

        /// <summary>
        /// Sammenligner konto indestående mellem crm og økonomi. Returnere SumAfstemning.
        /// </summary>
        public AfstemningSum DatoForMatchAfstemning(decimal crmKontoindestaaende, decimal oekonomiKontoindestaaende)
        {
            AfstemningSum afstemningSum = new AfstemningSum();

            List<AfstemningDatoSaldo> crmDatoSaldi = new List<AfstemningDatoSaldo>();
            crmDatoSaldi.Add(new AfstemningDatoSaldo { Dato = this.DatoTil.AddDays(1), Saldo = crmKontoindestaaende });

            List<AfstemningDatoSaldo> oekonomiDatoSaldi = new List<AfstemningDatoSaldo>();
            oekonomiDatoSaldi.Add(new AfstemningDatoSaldo { Dato = this.DatoTil.AddDays(1), Saldo = oekonomiKontoindestaaende });

            bool forsoegAfstemning = true;

            // Vi tæller dato ned for finde et evt. match
            while (forsoegAfstemning)
            {
                // Vi henter nogle flere saldi 
                crmDatoSaldi.Add(new AfstemningDatoSaldo { Dato = this.DatoTil, Saldo = HentIndestaaendeCRM(this.DatoTil) });
                oekonomiDatoSaldi.Add(new AfstemningDatoSaldo { Dato = this.DatoTil, Saldo = HentIndestaaendeOekonomi(this.DatoTil) * -1 });

                // Vi ser om vi kan finde et match saldo match mellem de 2 lister.
                // De nyeste datoer er øverst i liste. Så vi nyeste med saldo match.
                foreach (AfstemningDatoSaldo crmDatoSaldo in crmDatoSaldi)
                {
                    AfstemningDatoSaldo oekonomiDatoSaldo = oekonomiDatoSaldi.Where(ods => ods.Saldo == crmDatoSaldo.Saldo).FirstOrDefault();

                    // Fik vi et match på saldo
                    if (oekonomiDatoSaldo != null)
                    {
                        this.CRMContext.Trace.LogInformation($"Vi fandt overensstemmelse. Vi returner sidst afstemte ok dato til {this.DatoTil.Date}.");

                        afstemningSum.NyAfstemteDato = this.DatoTil;
                        afstemningSum.CRMDatoSaldo.Dato = this.DatoTil;
                        afstemningSum.CRMDatoSaldo.Saldo = crmKontoindestaaende;
                        afstemningSum.OekonomiDatoSaldo.Dato = this.DatoTil;
                        afstemningSum.OekonomiDatoSaldo.Saldo = oekonomiKontoindestaaende;
                        return afstemningSum;
                    }
                }

                if (this.DatoFra == this.DatoTil)
                {
                    this.CRMContext.Trace.LogInformation($"Vi fandt ikke overensstemmelse. Vi returnere dato for sidste overensstemmelse {this.DatoFra.AddDays(-1)}.");

                    afstemningSum.NyAfstemteDato = this.DatoFra.AddDays(-1);
                    afstemningSum.CRMDatoSaldo.Dato = DateTime.MinValue;
                    afstemningSum.CRMDatoSaldo.Saldo = crmKontoindestaaende;
                    afstemningSum.OekonomiDatoSaldo.Dato = DateTime.MinValue;
                    afstemningSum.OekonomiDatoSaldo.Saldo = oekonomiKontoindestaaende;
                    return afstemningSum;
                }

                // Vi forsøger med dagen før
                this.DatoTil = this.DatoTil.AddDays(-1);
            }

            throw new Exception("Logisk fejl i DatoForMatchAfstemning");
        }

        decimal HentIndestaaendeOekonomi(DateTime datoTil)
        {
            decimal oekonomiKontoindestaaende = 0;

            // SortedList<string, string> bsc = this.CRMContext.GetConfigSettingAll(Extending_CrmContext.BusinessCentralConfig);
            // BcContext bcContext = new BcContext(this.CRMContext.Trace, bsc);

            // 2022 08 02 RCL  Ændret bc til BcOAuthClientSecretContext

            SortedList<string, string> config = this.CRMContext.GetConfigSettingAll("Azure.BC.");
            BcOAuthClientSecretContext bcContext = new BcOAuthClientSecretContext(config);

            GLBalanceClient gLBalanceClient = new GLBalanceClient(bcContext);

            string filter = $"no eq '{this.KontoForIndestaaende}' and dateFilter_FilterOnly ge 2019-01-01 and dateFilter_FilterOnly le {datoTil.Year}-{datoTil.Month.ToString("00")}-{datoTil.Day.ToString("00")}";

            this.CRMContext.Trace.LogInformation($"Filter : {filter} GLBalanceClient.GetByFilter");

            GLBalanceListModel glBalanceListModel = gLBalanceClient.GetByFilter(filter);

            this.CRMContext.Trace.LogInformation($"Finder konto indestående");

            oekonomiKontoindestaaende = glBalanceListModel.items.First().totalAmount.Value;

            this.CRMContext.Trace.LogInformation($"Vi modtog BC {oekonomiKontoindestaaende} for dato til {datoTil.Date}.");

            return oekonomiKontoindestaaende;
        }

        decimal HentIndestaaendeCRM(DateTime datoTil)
        {
            decimal crmKontoindestaaende = 0;

            ap_finansposteringManager managerFinanspostering = new ap_finansposteringManager(this.CRMContext);
            decimal crmkontoindestaaende = managerFinanspostering.HentIndestaaende(new DateTime(datoTil.Year, 1, 1), new DateTime(datoTil.Year, datoTil.Month, datoTil.Day), true);

            this.CRMContext.Trace.LogInformation($"Vi modtog crm {crmKontoindestaaende} for dato til {datoTil.Date}.");

            return crmKontoindestaaende;
        }
    }
}
