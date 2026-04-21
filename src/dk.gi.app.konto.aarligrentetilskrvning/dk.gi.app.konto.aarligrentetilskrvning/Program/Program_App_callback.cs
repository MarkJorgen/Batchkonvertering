using dk.gi.cpr.servicelink;
using dk.gi.crm;
using dk.gi.crm.data.bll;
using dk.gi.crm.managers;
using dk.gi.crm.managers.V2;
using dk.gi.crm.models;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

//namespace dk.gi.app.console.template
namespace dk.gi.app.konto.aarligrentetilskrvning
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
            Trace.LogInformation("Starter årlig rentekørsel. CallBackFunction blev kaldt");

            string kontonrFra = string.Empty;

            Entity configurationSetting = null;

            using (ConfigurationSettingsManager managerConfiguration = new ConfigurationSettingsManager(this.crmcontext))
            {
                kontonrFra = managerConfiguration.Hent("app.konto.aarligrentetilskrvning.frakontonr");

                configurationSetting = managerConfiguration.Hent("app.konto.aarligrentetilskrvning.frakontonr", "config_configurationsettingid", "config_ntextcolumn", "modifiedon");

                if (kontonrFra != "41-00001")
                {
                    DateTime modifiedon = configurationSetting.GetAttributeValue<DateTime>("modifiedon");
                    if (modifiedon.AddDays(5) < DateTime.Now)
                    {
                        Trace.LogInformation($"ConfigurationSettings: app.konto.aarligrentetilskrvning.frakontonr har værdien {kontonrFra} bør være 41-00001");
                        Trace.LogError($"Fejl configurationSettings: app.konto.aarligrentetilskrvning.frakontonr har værdien {kontonrFra} bør være 41-00001...");
                        return AppStatus.StateCode.AppUventetFejlIProgramKode;
                    }
                }

                Trace.LogInformation($"kontonr fra {kontonrFra}");
            }

            Konto konto = new Konto();
            List<KontoLinje> kontiTilbehandling = konto.HentAlle(this.Trace, this.crmcontext).OrderBy(k => k.KontoNr).ToList();

            int aar = DateTime.Today.Year;
            string udbetalingsKode = System.Configuration.ConfigurationManager.AppSettings["UdbetalingsKodeAarligRente"];
            string finanspostTekst = System.Configuration.ConfigurationManager.AppSettings["FinanspostTekstAarligRente"];

            Finanssaldo finanssaldo = new Finanssaldo();
            List<FinanssaldoLinje> finanssaldi = finanssaldo.HentAlle(this.Trace, this.crmcontext, aar);

            // Vi skal lige bruge kontosystemværdier
            Ap_KontoSystemManager kontosystemManager = new Ap_KontoSystemManager(this.crmcontext);
            KontosystemVaerdier kontosystemVaerdier = kontosystemManager.Vaerdier();

            List<KontoRenteLinje> kontiRenteLinjer = new List<KontoRenteLinje>();

            Trace.LogInformation($"Beregner rentelinjer for konti...");

            foreach (KontoLinje kontoLinje in kontiTilbehandling)
            {
                KontoRenteLinje kontoRenteLinje = finanssaldo.AarsRente(this.Trace, kontoLinje, finanssaldi);
                if (kontoRenteLinje != null)
                {
                    kontiRenteLinjer.Add(kontoRenteLinje);
                }
            }

            Trace.LogInformation($"Beregning af rentelinjer er foretaget...");

            if (kontonrFra != "99-99999")
            {
                Finanspost finanspost = new Finanspost();
                PrimoFinansSaldo primoFinansSaldo = new PrimoFinansSaldo();

                Trace.LogInformation($"Opretter finansposter og primosaldi for konti i CRM...");

                string kontonr = string.Empty;

                using (ap_finansposteringManager finansposteringManager = new ap_finansposteringManager(this.crmcontext))
                {
                    try
                    {
                        foreach (KontoLinje kontoLinje in kontiTilbehandling)
                        {
                            kontonr = kontoLinje.KontoNr;

                            if (!string.IsNullOrEmpty(kontonrFra) && string.Compare(kontonr, kontonrFra, StringComparison.Ordinal) <= 0)
                            {
                                if (kontonr != kontonrFra)
                                    continue;
                            }

                            if (kontiRenteLinjer.Exists(krl => krl.KontoId == kontoLinje.KontoId))
                            {
                                KontoRenteLinje kontoRenteLinje = kontiRenteLinjer.Where(krl => krl.KontoId == kontoLinje.KontoId).First();

                                finanspost.Opret(this.Trace, finansposteringManager, kontoRenteLinje, udbetalingsKode, finanspostTekst, aar);
                                primoFinansSaldo.Opret(this.Trace, this.crmcontext, kontoRenteLinje.KontoId, kontoRenteLinje.KontoNr, aar);
                            }
                            else
                            {
                                // 2021 01 11 RCL Er kontoindestånde større end 0 og ingen rentelinje skal vi oprette primosaldo efter aftale med HRL
                                if (kontoLinje.Kontoindestaaende > 0 && !kontiRenteLinjer.Exists(krl => krl.KontoId == kontoLinje.KontoId))
                                {
                                    primoFinansSaldo.Opret(this.Trace, this.crmcontext, kontoLinje.KontoId, kontoLinje.KontoNr, aar);
                                }
                            }
                        }
                    }
                    catch
                    {
                        Configuration.Saet(Trace, crmcontext, configurationSetting, kontonr);

                        Trace.LogError($"Fejlede på konto {kontonr} tjek finanspostering/finanssaldo for denne...");
                        Trace.LogInformation($"ConfigurationSettings: app.konto.aarligrentetilskrvning.frakontonr er sat til det kontonr, som kørsel skal genstartes fra formentlig {kontonr}");
                        Trace.LogError($"Fejlede på konto {kontonr} tjek finanspostering/finanssaldo...");
                        return AppStatus.StateCode.AppUventetFejlIProgramKode;
                    }
                }

                Trace.LogInformation($"Finansposter og primosaldi for konti i CRM er oprettet...");
            }

            // Opret kladde i BC og bogføre denne for kontoIntervalSekvens
            Trace.LogInformation($"Opretter kladde i BC...");

            CrmContext _crmcontext = new dk.gi.crm.CrmContext(appConfig.GetCrmConnectionString);

            try
            {
                BC bc = new BC(_crmcontext, this.Trace);
                bc.BogfoerRente(_crmcontext, aar, kontiRenteLinjer, kontosystemVaerdier.KontoForIndestaaende, kontosystemVaerdier.RenteKonto, AzureBC);
            }
            catch
            {
                Configuration.Saet(Trace, _crmcontext, configurationSetting, "99-99999");

                Trace.LogError($"Bogføring fejlede i bc...");
                Trace.LogInformation($"Kontroller at configurationSettings: app.konto.aarligrentetilskrvning.frakontonr er sat til 99-99999 inden bogføring i bc forsøges gentaget...");
                Trace.LogError($"Bogføring fejlede i bc...");
                return AppStatus.StateCode.AppUventetFejlIProgramKode;
            }

            Trace.LogInformation($"Kladde er oprettet i BC...");

            Trace.LogInformation($"Årlig rentekørsel er afviklet. CallBackFunction slut {result}");

            Configuration.Saet(Trace, _crmcontext, configurationSetting, "41-00001");

            return result;
        }
    }
}