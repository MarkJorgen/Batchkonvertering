using dk.gi.asbq;
using dk.gi.crm;
using dk.gi.crm.data.bll;
using dk.gi.crm.managers;
using dk.gi.crm.models;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using dk.gi.email;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

//namespace dk.gi.app.console.template
namespace dk.gi.app.konto.indberetskat
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

            DateTime forrigeRelationsAarMedtagesFraDato;
            string emailModtager = string.Empty;
            Entity configurationSetting = null;

            using (ConfigurationSettingsManager managerConfiguration = new ConfigurationSettingsManager(this.crmcontext))
            {
                try
                {
                    Trace.LogInformation("app.konto.indberetskat.email.modtager");

                    emailModtager = managerConfiguration.Hent("app.konto.indberetskat.email.modtager");

                    if (string.IsNullOrWhiteSpace(emailModtager))
                    {
                        Trace.LogError("app.konto.indberetskat.email.modtager er tom");
                        return AppStatus.StateCode.AppUventetFejlIProgramKode;
                    }
                }
                catch
                {
                    Trace.LogError("$Fejl i app.konto.indberetskat.email.modtager.");
                    return AppStatus.StateCode.AppUventetFejlIProgramKode;
                }

                try
                {
                    Trace.LogInformation("app.konto.indberetskat.forrige.relations.aar.medtages.fra.dato");

                    configurationSetting = managerConfiguration.Hent("app.konto.indberetskat.forrige.relations.aar.medtages.fra.dato", "config_configurationsettingid", "config_ntextcolumn", "modifiedon");

                    string[] _forrigeRelationsAarMedtagesFraDato = managerConfiguration.Hent("app.konto.indberetskat.forrige.relations.aar.medtages.fra.dato").Split('-');

                    forrigeRelationsAarMedtagesFraDato = new DateTime(Convert.ToInt32(_forrigeRelationsAarMedtagesFraDato[2]), Convert.ToInt32(_forrigeRelationsAarMedtagesFraDato[1]), Convert.ToInt32(_forrigeRelationsAarMedtagesFraDato[0]));

                    if (forrigeRelationsAarMedtagesFraDato < new DateTime(DateTime.Now.Year - 1, 8, 1))
                    {
                        Trace.LogError($"app.konto.indberetskat.forrige.relations.aar.medtages.fra.dato skal værer større end lig med: {new DateTime(DateTime.Now.Year - 1, 8, 1).ToShortDateString()}");
                        return AppStatus.StateCode.AppUventetFejlIProgramKode;
                    }
                }
                catch
                {
                    Trace.LogError($"Fejl i app.konto.indberetskat.forrige.relations.aar.medtages.fra.dato, skal være på formen dd-MM-yyyy.");
                    return AppStatus.StateCode.AppUventetFejlIProgramKode;
                }
            }

            try
            {
                int indberetningsAar = DateTime.Now.Year - 1;

                // Skal der køres en enkelt konto, nej
                string txtKonto = string.Empty;

                // Skal vi stoppe ved fejl, nej
                bool stopVedFejl = false;

                // Opret filer vi skal benytte til vores resultat og fejl
                StringBuilder filFejl = new StringBuilder();
                StringBuilder filData = new StringBuilder();

                Trace.LogInformation("Hent data for alle konti, disse vil senere blive begrænset...");

                // definition af om en fejl opstod (benyttes da vi har et dobbelt-loop) til at stoppebehandlingen
                bool stopVidereBehandling = false;

                // Definition af konti der skal behandles
                List<HentKontiItem[]> konti = null;

                #region Find konti der skal behandles
                // Kør forespørgsel til at hente alle konti
                HentKontiRequest kontiRequest = new HentKontiRequest(crmcontext)
                {
                    // Angiv GI's CVR nummer for at få alle konti, vi vil senere frasortere konti, såfremt brugeren                        // har angivet en begrænsning
                    KundeId = "26092515"
                };

                HentKontiResponse kontiSvar = kontiRequest.Execute<HentKontiResponse>();

                // Fik vi et svar på vores søgning efter konti

                if (kontiSvar.Status.StateCode != IResponseState.ResponseStateCodes.OK)
                {
                    throw new Exception($"Fejlbesked: {kontiSvar.Status.Message ?? ""}");
                }

                //
                Trace.LogInformation($"Fandt totalt {kontiSvar.Konti?.Length} konti i CRM, begrænser disse til de angivne begrænsninger, er der ingen begrænser vælges alle 41-, 42-, 43-, 44-, 45- og 46-konti.");

                // Byg et array af RegularExpression objekter, til at vælge de konti, der skal behandles
                var kontiUdvaelgRegulaereUdtryk = (txtKonto.Trim().Length > 0 ? txtKonto : $"41-*{Environment.NewLine}42-*{Environment.NewLine}43-*{Environment.NewLine}44-*{Environment.NewLine}45-*{Environment.NewLine}46-*")
                    .Trim()
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => new Regex(t.Replace("?", "[\\d-]?").Replace("*", "[\\d-]*")))
                    .ToArray();

                // Vælg alle de konti der skal benyttes til den aktuelle kørsel
                konti = kontiSvar.Konti
                    .Where(k => kontiUdvaelgRegulaereUdtryk.Any(ku => ku.IsMatch(k.KontoNr)))
                    .OrderBy(k => k.KontoNr)
                    .ToBatchList(300);

                // Gem antallet af konti
                int kontiAntal = konti.Sum(k => k.Length);
                //
                Trace.LogInformation($"Efter begrænsningen er der {kontiAntal} konti tilbage til behandling.");

                // Såfremt der ikke er nogle konti, så stopper vi behandlingen her
                if (kontiAntal == 0)
                {
                    Trace.LogInformation("Der var ingen konti der blev behandlet!");
                    return result;
                }
                #endregion

                // 2020 06 08 RCL Filler til linje(r)
                string filler = "";

                // 2020 06 08 RCL Start individ skal nu udskrives
                int i = 0;
                int antal = 0;

                for (i = 1; i <= 204; i++)
                {
                    filler += " ";
                }

                Trace.LogInformation($"Udskriver start record.");

                // INDVNR 1 4 N Konstant: 0001
                // INDSENDERSENR 5 8 N Indsenders SENR
                // FILNR 13 10 N Konstant: 0314062280
                // INDSENDERS NAVN 23 27 A Indsenders navn
                // INDSENDERS ADRESSE 50 35 A Indsenders adresse
                // FILLER 85 204 A Konstant: blank
                //                          1         2         3         4         5         6         7         8         9         
                //                 123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
                filData.AppendLine("0001260925150314062280Grundejernes InvesteringsfoNy Kongensgade 15, 1472 København K" + filler);

                filler = "";

                // Filler til linjer
                // 2023 01 09 RCL Changed to 175 
                for (i = 1; i <= 175; i++)
                {
                    filler += " ";
                }

                foreach (var kontoBatch in konti)
                {
                    // Hent data for næste gruppe af konti
                    HentIndberetSkatInformationRequest kontoBatchReq = new HentIndberetSkatInformationRequest(crmcontext)
                    {
                        Aar = indberetningsAar,
                        KontoIdSamling = kontoBatch.Select(k => k.Id).ToArray(),
                        ForrigeRelationsAarMedtagesFraDato = forrigeRelationsAarMedtagesFraDato
                    };
                    HentIndberetSkatInformationResponse kontoBatchData = kontoBatchReq.Execute<HentIndberetSkatInformationResponse>();

                    #region Validering
                    // Gik det godt at hente vores data for den aktuelle konto?
                    if (kontoBatchData.Status.StateCode != IResponseState.ResponseStateCodes.OK)
                    {
                        // ... nej, vi fik en fejl eller en advarsel
                        //
                        Trace.LogError($"Fejl under forsøget på at hente data for konti {String.Join(", ", kontoBatch.Select(k => k.KontoNr))}, fejlbesked: {kontoBatchData.Status.Message})");

                        // Skriv til vores fejl-fil
                        filFejl.AppendLine(kontoBatchData.Status.Message);

                        // Har brugeren bedt om at få afbrydt behandlingen, når der opstår en fejl.
                        if (stopVedFejl)
                            break;

                        // Fortsæt til næste konto
                        continue;
                    }
                    #endregion

                    foreach (var batchKonto in kontoBatch)
                    {
                        // Tillad at UI opdateres
                        Application.DoEvents();

                        var kontoData = kontoBatchData.Konti.FirstOrDefault(k => k.Id == batchKonto.Id);

                        //
                        Trace.LogInformation($"Behandler konto {batchKonto.KontoNr} (ID: {batchKonto.Id})");

                        #region Tjek om der opstod en fejl eller om der mangler data i vores kontoData
                        // Er der registreret fejl under fremsøgning af data
                        if (kontoData.Fejl != null && kontoData.Fejl.Any())
                        {
                            foreach (string fejl in kontoData.Fejl)
                            {
                                // Skriv til vores fejl-fil
                                filFejl.AppendLine(fejl);
                            }

                            // Har brugeren bedt om at få afbrydt behandlingen, når der opstår en fejl.
                            if (stopVedFejl)
                            {
                                stopVidereBehandling = true;
                                break;
                            }
                        }

                        // såfremt der ikke er kontoejere, så fortsæt til næste konto
                        if (kontoData.Kontoejere == null || !kontoData.Kontoejere.Any())
                            continue;
                        #endregion

                        // Foretag behandling for alle kontoejerne
                        foreach (var kontoejer in kontoData.Kontoejere)
                        {
                            #region Validering af ejerNr (kontoejer.CPR, kontoejer.CVR og kontoejer.SE)
                            if ((kontoejer.CPR.Length > 0 && (kontoejer.CVR.Length > 0 || kontoejer.SE.Length > 0)) || (kontoejer.CVR.Length > 0 && kontoejer.SE.Length > 0))
                            {
                                string fejlbesked = $"Konto {batchKonto.KontoNr}. Flere ejernr. felter er udfyldt. Har registretet følgende CPR: {kontoejer.CPR}, CVR: {kontoejer.CVR} og SE: {kontoejer.SE}";
                                // ... log fejl
                                filFejl.AppendLine(fejlbesked);

                                if (stopVedFejl)
                                {
                                    stopVidereBehandling = true;
                                    break;
                                }
                                else
                                {
                                    // Der er udfyldt i hver fald 2 af de 3 ejernr. felter, og da vi foretrække SE, så CVR og til sidst CPR... så
                                    // kan vi nulstille CPR.
                                    kontoejer.CPR = "";
                                    // ... og så nulstille CVR, såfremt der er indhold i SE.
                                    kontoejer.CVR = kontoejer.SE.Length > 0 ? "" : kontoejer.CVR;
                                }
                            }

                            if ((kontoejer.CPR.Length > 0 && kontoejer.CPR.Length != 10) || (kontoejer.CVR.Length + kontoejer.SE.Length != 0 && kontoejer.CVR.Length + kontoejer.SE.Length != 8))
                            {
                                string fejlbesked = $"Konto {batchKonto.KontoNr}. Forkert længde på ejernr. Har registretet følgende CPR: {kontoejer.CPR}, CVR: {kontoejer.CVR} og SE: {kontoejer.SE}";
                                // ... log fejl
                                filFejl.AppendLine(fejlbesked);

                                if (stopVedFejl)
                                {
                                    stopVidereBehandling = true;
                                    break;
                                }
                            }
                            #endregion

                            #region Validering af skattekode
                            if (kontoejer.Skattekode.HasValue == false)
                            {
                                // ... log fejl
                                filFejl.AppendLine($"Konto {batchKonto.KontoNr}. Skattekode mangler.");

                                if (stopVedFejl)
                                {
                                    stopVidereBehandling = true;
                                    break;
                                }
                            }
                            #endregion

                            #region Validering af landekode
                            if (kontoejer.Skattekode.GetValueOrDefault() == 2 && string.IsNullOrWhiteSpace(kontoejer.Landekode))
                            {
                                // ... log fejl
                                filFejl.AppendLine($"Konto {batchKonto.KontoNr}. Landekode mangler.");

                                if (stopVedFejl)
                                {
                                    stopVidereBehandling = true;
                                    break;
                                }
                            }

                            // Hvis landekode er lig med DK så vil vi ikke have den med i resultat
                            if (string.IsNullOrWhiteSpace(kontoejer.Landekode) == false && kontoejer.Landekode.Trim().ToUpper() == "DK")
                                kontoejer.Landekode = "   ";
                            #endregion

                            string ejerNr = (kontoejer.CPR + kontoejer.CVR + kontoejer.SE).PadLeft(10, '0');
                            string bbr = ("").PadLeft(9, '0');
                            // 2025 01 31 RCL Removed bbr
                            // string bbr = (kontoData.BbrNr ?? "").PadLeft(9, '0');
                            string bindingstype = kontoData.Bindingstype;
                            string ejerandel = Math.Round(kontoejer.Ejerandel, 0).ToString().PadLeft(3, '0');
                            string ejerskiftekode = kontoejer.Ejerskiftedato.HasValue ? "o" : "0";
                            string skattekode = (kontoejer.Skattekode.Value - 1).ToString();
                            string landekode = kontoejer.Landekode.PadRight(3);
                            string harAktivitet = Convert.ToInt32(kontoData.HarAktivitet).ToString();
                            string startmaaned = kontoData.Regnskabstartmaaned.ToString().PadLeft(2, '0');
                            string indbetalt = Convert.ToInt32(Math.Round((kontoData.AxaptaData?.Indbetalt).GetValueOrDefault(), 0) * -1).ToString().PadLeft(8, '0');

                            // 2020 06 16 RCL Altid positivt efter aftalse med HRL
                            string udbetalt = Convert.ToInt32(Math.Abs(Math.Round((kontoData.AxaptaData?.Udbetalt).GetValueOrDefault(), 0))).ToString().PadLeft(8, '0');

                            // 2021 07 02 RCL Skal ikke benyttes mere efter aftale med HRL
                            string t1 = " ";

                            // 2020 06 16 RCL Altid positivt efter aftalse med HRL
                            string udbetalt2 = Convert.ToInt32(Math.Abs(Math.Round((kontoData.AxaptaData?.Udbetalt2).GetValueOrDefault(), 0))).ToString().PadLeft(8, '0');

                            // 2021 07 02 RCL Skal ikke benyttes mere efter aftale med HRL
                            string t2 = " ";

                            // 2020 06 16 RCL Altid positivt efter aftalse med HRL
                            string udbetalt3 = Convert.ToInt32(Math.Abs(Math.Round((kontoData.AxaptaData?.Udbetalt3).GetValueOrDefault(), 0))).ToString().PadLeft(8, '0');

                            string modregnet = Convert.ToInt32(Math.Round((kontoData.BindingspligtPrimo < 0 ? kontoData.Hensaettelser : 0), 0)).ToString().PadLeft(8, '0');

                            // 2020 06 16 RCL Altid positivt efter aftalse med HRL
                            string hensaettelser = Convert.ToInt32(Math.Abs(Math.Round(kontoData.Hensaettelser, 0))).ToString().PadLeft(8, '0');

                            string bindingspligtfortegn = kontoData.BindingspligtUltimo < 0 ? "-" : "+";
                            string bindingspligtUltimo = Convert.ToInt32(Math.Abs(Math.Round(kontoData.BindingspligtUltimo, 0))).ToString().PadLeft(9, '0');
                            string indestaaendeUltimo = Convert.ToInt32(Math.Round((kontoData.AxaptaData?.IndestaaendeUltimoAaret).GetValueOrDefault(), 0) * -1).ToString().PadLeft(8, '0');

                            // 2024 01 09 RCL Added BFE
                            string bfenr = (kontoData.BFENummer).ToString().PadLeft(10, '0');

                            // 2020 06 08 RCL Gammel struktur
                            //string linje = $" 1360 {ejerNr} {bbr} {bindingstype} {ejerandel} {ejerskiftekode} {skattekode} {landekode} {harAktivitet} {startmaaned} {indbetalt} " +
                            //    $"{udbetalt} {t1} {udbetalt2} {t2} {udbetalt3} {modregnet} {hensaettelser} {bindingspligtfortegn} {bindingspligtUltimo} {indestaaendeUltimo}  ";

                            // 2020 06 08 RCL Fjernet blanke og ændret filler størelse
                            string linje = $"1360{ejerNr}{bbr}{bindingstype}{ejerandel}{ejerskiftekode}{skattekode}{landekode}{harAktivitet}{startmaaned}{indbetalt}" +
                                $"{udbetalt}{t1}{udbetalt2}{t2}{udbetalt3}{modregnet}{hensaettelser}{bindingspligtfortegn}{bindingspligtUltimo}{indestaaendeUltimo}{bfenr}" +
                                filler;

                            antal++;

                            filData.AppendLine(linje);
                        }
                    }

                    if (stopVidereBehandling)
                        break;
                }

                filler = "";

                for (i = 1; i <= 269; i++)
                {
                    filler += " ";
                }

                Trace.LogInformation($"Udskriver slut record {antal} og med foran stillede 0 {antal.ToString().PadLeft(7, '0')}.");


                // Vi skal lægge 2 til for start/slut record
                antal = antal + 2;

                // INDVNR 1 4 N Konstant: 9998
                // INDSENDERSENR 5 8 N Samme som i individ 0001
                // ANTAL INDIVIDER 13 7 N Antal indberettede individer incl.start - og slutindivid. Bemærk: Skal højrestilles med foranstillede nuller.
                // FILLER 20 269 A Konstant: blank
                //                          1         2         3         4         5         6 
                //                 123456789012345678901234567890123456789012345678901234567890
                filData.AppendLine("999826092515" + antal.ToString().PadLeft(7, '0') + filler);

                using (ConfigurationSettingsManager configManager = new ConfigurationSettingsManager(crmcontext))
                {
                    string EmailClientId = appConfig.EmailClientId;
                    string EmailClientSecret = appConfig.EmailClientSecret;
                    string EmailTenantid = appConfig.EmailTenantid;
                    string EmailAfsenderMailAdressse = appConfig.EmailAfsenderMailAdressse;
                    string[] EmailModtagere = new string[] { emailModtager };
                    string CrmServerName = crmcontext.GetCrmServerName();


                    List<dk.gi.email.EmailAttachment> emailAttachment = new List<dk.gi.email.EmailAttachment>();


                    Encoding enc = Encoding.GetEncoding(1252);
                    string filDataBase64 = Convert.ToBase64String(enc.GetBytes(filData.ToString()));
                    emailAttachment.Add(new EmailAttachment { Name = $"{indberetningsAar}_IndberetSkat.txt", ContentType = "text/plain", IndholdBase64 = filDataBase64 });

                    byte[] filFejlAsciiBytes = null;
                    string filFejlAsciiString = string.Empty;

                    if (filFejl.Length > 0)
                    {
                        // Convert string to ASCII bytes
                        filFejlAsciiBytes = Encoding.ASCII.GetBytes(filFejl.ToString());

                        // Convert ASCII bytes back to string
                        filFejlAsciiString = Encoding.ASCII.GetString(filFejlAsciiBytes);

                        emailAttachment.Add(new EmailAttachment { Name = $"{indberetningsAar}_IndberetSkat_Fejl.txt", ContentType = "text/plain", IndholdTekst = filFejlAsciiString });
                    }

                    string fejl = filFejl.Length == 0 ? " Indberet skat kørsel kørte igennem uden fejl." : "";

                    // Set Email content
                    string subject = $"dk.gi.app.konto.indberetskat, Crm:{CrmServerName}, Dato:{System.DateTime.Now.ToString("yyyy-MM-dd hh:mm")}";
                    string body = $"Se vedhæftede file(r) for indberet skat kørsel.\r\n\r\nBenyttet dato værdi fra app.konto.indberetskat.forrige.relations.aar.medtages.fra.dato: {forrigeRelationsAarMedtagesFraDato.ToShortDateString()}.{fejl}\r\n\r\nDette er en automatisk genereret mail, fra {CrmServerName}.\r\n";

                    dk.gi.email.EmailContext eContext = new dk.gi.email.EmailContext(EmailClientId, EmailClientSecret, EmailTenantid, EmailAfsenderMailAdressse);

                    // Send mail
                    if (dk.gi.email.EmailClient.SendEmail(eContext, subject, body, EmailModtagere, false, emailAttachment) == false)
                    {
                        Trace.LogError("Kunne ikke sende mail");

                        return AppStatus.StateCode.AppExceptionInCode;
                    }
                }
            }
            catch (Exception ex)
            {
                //
                Trace.LogError(ex.ToString());
            }
            finally
            {
                //
                Trace.LogInformation("Afslutter kørsel");
            }

            Trace.LogInformation("Opdaterer configuration");

            try
            {
                if (DateTime.Today >= new DateTime(2025, 8, 15))
                {
                    using (ConfigurationSettingsManager managerConfiguration = new ConfigurationSettingsManager(crmcontext))
                    {
                        configurationSetting.Attributes["config_ntextcolumn"] = $"{DateTime.Now.Day.ToString("D2")}-{DateTime.Now.Month.ToString("D2")}-{DateTime.Now.Year.ToString()}";
                        managerConfiguration.Update(configurationSetting);
                    }
                }
            }
            catch
            {
                Trace.LogError($"Fejl ved opdatering af configuration setting.");
                return AppStatus.StateCode.AppExceptionInCode;
            }

            Trace.LogInformation($"CallBackFunction slut {result}");

            return result;
        }
    }
}