using dk.gi.bc.servicelink;
using dk.gi.crm.giproxy;
using dk.gi.crm.managers.V2;
using dk.gi.crm.models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace dk.gi.crm.app.konto.afstemfinansposter
{
    public class AabneposterBC
    {
        int taeller = 0;

        AfstemningSum AfstemningSum { get; set; }

        CrmContext CRMContext { get; set; }

        List<AfstemningPostering> CRMPosteringer = new List<AfstemningPostering>();

        List<AfstemningPostering> OekonomiPosteringer = new List<AfstemningPostering>();
        string KontoForIndestaaende { get; set; }

        public AabneposterBC(CrmContext crmContext, AfstemningSum afstemningSum)
        {
            this.CRMContext = crmContext;
            this.AfstemningSum = afstemningSum;
            Ap_KontoSystemManager managerKontoSystem = new Ap_KontoSystemManager(this.CRMContext);
            this.KontoForIndestaaende = managerKontoSystem.Vaerdier().KontoForIndestaaende;
        }

        public List<AfstemningPostering> Afstem()
        {
            // Hent åbne poster crm
            InitOekonomiPosteringer();

            // Hent åbne poster økonomi
            InitCRMPosteringer();

            // Vi samler åbne posteringer fra crm og økonomi og sortere dem på dato, beløb og system  
            List<AfstemningPostering> afstemtePosteringer = new List<AfstemningPostering>();
            afstemtePosteringer.AddRange(this.CRMPosteringer);
            afstemtePosteringer.AddRange(this.OekonomiPosteringer);
            afstemtePosteringer = afstemtePosteringer.OrderBy(p => p.Posteringsdato).OrderBy(p => p.Beloeb).OrderBy(p => p.System).ToList();

            return afstemtePosteringer;
        }

        void InitOekonomiPosteringer()
        {
            DateTime fraDato = this.AfstemningSum.OekonomiDatoSaldo.Dato != DateTime.MinValue ? this.AfstemningSum.OekonomiDatoSaldo.Dato :
                // RCL Vi skal ikke have gamle poster med der er afstemt. Derfor lægger vi en dag til    
                this.AfstemningSum.NyAfstemteDato.AddDays(1);

            //SortedList<string, string> bsc = this.CRMContext.GetConfigSettingAll(Extending_CrmContext.BusinessCentralConfig);
            //BcContext bcContext = new BcContext(this.CRMContext.Trace, bsc);
            
            // 2022 08 02 RCL  Ændret bc til BcOAuthClientSecretContext
            
            SortedList<string, string> config = this.CRMContext.GetConfigSettingAll("Azure.BC.");
            BcOAuthClientSecretContext bcContext = new BcOAuthClientSecretContext(config);

            GLEntryClient glEntryClient = new GLEntryClient(bcContext);

            string filter = $"accountNo eq '{this.KontoForIndestaaende}' and postingDate ge {fraDato.Year}-{fraDato.Month.ToString("00")}-{fraDato.Day.ToString("00")} and postingDate le {fraDato.Year}-12-31";

           this.CRMContext.Trace.LogInformation($"Filter : {filter} GLEntryClient.GetByFilter");

            GLEntryListModel glEntryListModel = glEntryClient.GetByFilter(filter);

            foreach (GLEntryModel glEntryModel in glEntryListModel.items.OrderBy(e => e.postingDate))
            {
                OekonomiPosteringer.Add(new AfstemningPostering
                {
                    Id = taeller++,
                    System = "AX",
                    Beloeb = glEntryModel.amount.Value,
                    Tekst = glEntryModel.description,
                    Posteringsdato = glEntryModel.postingDate.Value,
                    Produktgruppe = ""
                });
            }
        }

        void InitCRMPosteringer()
        {

            DateTime fraDato = this.AfstemningSum.CRMDatoSaldo.Dato != DateTime.MinValue ? this.AfstemningSum.CRMDatoSaldo.Dato :
                // RCL Vi skal ikke have gamle poster med der er afstemt. Derfor lægger vi en dag til    
                this.AfstemningSum.NyAfstemteDato.AddDays(1);

            ap_finansposteringManager managerFinanspostering = new ap_finansposteringManager(this.CRMContext);
            List<ap_finanspostering> finansposteringer = managerFinanspostering.HentForPeriode(fraDato, new DateTime(fraDato.Year, 12, 31), (int)AP_udbetalingskode_ap_posteringstype.Indestående,
                ap_finanspostering.Fields.ap_belob, ap_finanspostering.Fields.ap_posteringsdato, ap_finanspostering.Fields.ap_name, ap_finanspostering.Fields.ap_kontoId)
                .OrderBy(p => p.ap_posteringsdato).ToList();

            Ap_KontoManager kontoManager = new Ap_KontoManager(this.CRMContext);

            foreach (ap_finanspostering finanspostering in finansposteringer)
            {
                string kontonr = kontoManager.Hent(finanspostering.ap_kontoId.Id, AP_konto.Fields.AP_Kontonr).AP_Kontonr;

                CRMPosteringer.Add(new AfstemningPostering
                {
                    Konto = kontonr,
                    Posteringsdato = finanspostering.ap_posteringsdato.Value.ToLocalTime(),
                    Tekst = finanspostering.ap_name,
                    System = "CRM",
                    Beloeb = finanspostering.ap_belob.Value,
                    Id = taeller++
                });
            }
        }

        #region private hjælpe metoder
        private static DateTime ConvertTextToDateTime(string text)
        {
            if (String.IsNullOrWhiteSpace(text))
                return DateTime.MinValue;

            var result = DateTime.MinValue;

            if (DateTime.TryParse(text, System.Globalization.CultureInfo.GetCultureInfo("da-DK"), System.Globalization.DateTimeStyles.None, out result))
                return result;

            if (DateTime.TryParse(text, System.Globalization.CultureInfo.GetCultureInfo("en-US"), System.Globalization.DateTimeStyles.None, out result))
                return result;

            if (DateTime.TryParse(text, System.Globalization.CultureInfo.GetCultureInfo("en-GB"), System.Globalization.DateTimeStyles.None, out result))
                return result;

            return DateTime.MinValue;
        }

        private static decimal ConvertTextToDecimal(string text)
        {
            if (String.IsNullOrWhiteSpace(text))
                return 0M;

            var result = 0M;

            if (decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("da-DK"), out result))
                return result;

            if (decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out result))
                return result;

            if (decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-GB"), out result))
                return result;

            return result;
        }
        #endregion
    }
}
