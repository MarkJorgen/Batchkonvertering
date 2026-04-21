using dk.gi.cpr.servicelink;
using dk.gi.crm;
using dk.gi.crm.data.DigitalPost;
using dk.gi.crm.giproxy;
using dk.gi.crm.managers.V2;
using dk.gi.crm.models;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dk.gi.app.konto.orientering.sletning
{
    public class KontoOrienteringSletning
    {
        /// <summary>
        /// Crm forbindelse
        /// </summary>
        CrmContext CrmContext { get; set; }

        /// <summary>
        /// En privat logger som default er sat til en NullLogger, den rettes/sættes så i Konstructor
        /// </summary>
        internal Microsoft.Extensions.Logging.ILogger Trace { get; private set; } = NullLogger.Instance;

        /// <summary>
        /// Send digital post ja/nej
        /// </summary>
        bool TilladSendTilDigitalPost { get; set; }

        /// <summary>
        /// Dato til afgrænsing på konto
        /// </summary>
        DateTime DatoOprettet { get; set; }

        /// <summary>
        /// Cvr nummer på kalder
        /// </summary>
        string KundeId { get; set; }

        /// <summary>
        /// Navn på bruger
        /// </summary>
        string BrugerNavn { get; set; }

        /// <summary>
        /// Emne navn til sag
        /// </summary>
        string SagsEmne { get; set; }

        /// <summary>
        /// Type til sag
        /// </summary>
        Guid SagsType { get; set; }

        /// <summary>
        /// Benyttet aktivitets Kode til oprettet aktivitet
        /// </summary>
        string OpgaveAktivitetskode { get; set; }

        /// <summary>
        /// OpgaveEmne navn til aktivitets oprettelse
        /// </summary>
        string OpgaveAktivitetEmne { get; set; }

        /// <summary>
        /// Svarfrist til brev
        /// </summary>
        int SvarFrist { get; set; }

        public KontoOrienteringSletning(CrmContext crmContext, Microsoft.Extensions.Logging.ILogger trace, bool tilladSendTilDigitalPost, DateTime datoOprettet, string kundeId, string brugerNavn,
            string sagsEmne, Guid sagsType, string opgaveAktivitetskode, string opgaveAktivitetEmne, int svarFrist)
        {
            this.CrmContext = crmContext;
            this.Trace = trace; 
            this.TilladSendTilDigitalPost = tilladSendTilDigitalPost;
            this.DatoOprettet = datoOprettet;
            this.KundeId = kundeId;
            this.BrugerNavn = brugerNavn;
            this.SagsEmne = sagsEmne;
            this.SagsType = sagsType;
            this.OpgaveAktivitetskode = opgaveAktivitetskode;
            this.OpgaveAktivitetEmne = opgaveAktivitetEmne;
            this.SvarFrist = svarFrist;
        }

        public bool Afvikling()
        {
            string kontoNr = null;

            try
            {
                using (Ap_RegnskabManager regnskabManager = new Ap_RegnskabManager(this.CrmContext))
                using (Ap_KontoManager kontoManager = new Ap_KontoManager(this.CrmContext))
                using (Ap_KontoManager kontoManagerUpdate = new Ap_KontoManager(this.CrmContext))
                using (ContactManager kontaktPersonManager = new ContactManager(this.CrmContext))
                using (AP_ejendomManager ejendomManager = new AP_ejendomManager(this.CrmContext))
                {
                    int antal = 0;

                    List<AP_konto> konti = kontoManager.HentAktiveLovgrundlagOprettetFra((int)AP_konto_ap_lovgrundlag.Efterjuni2015, this.DatoOprettet, AP_konto.Fields.Id, AP_konto.Fields.AP_Kontonr,
                        AP_konto.Fields.ap_primrkontaktid, AP_konto.Fields.ap_ejendomid);

                    foreach (AP_konto konto in konti)
                    {
                        kontoNr = konto.AP_Kontonr;

                        AP_regnskab regnskab = regnskabManager.HentAlleRegnskaber(konto.Id, null, AP_regnskab.Fields.Id).ToList().FirstOrDefault();

                        if (regnskab == null)
                        {
                            antal++;

                            this.Trace.LogInformation($"Denne konto {kontoNr} vil blive behandlet. I alt behandlet {antal}");

                            //Ingen regnskaber til konto
                            AP_ejendom ejendom = ejendomManager.Hent(konto.ap_ejendomid.Id, AP_ejendom.Fields.AP_samletadresse);

                            // Opret sag
                            string sagsnummer = this.OpretSag(konto);

                            Contact kontaktPerson = kontaktPersonManager.Hent(konto.ap_primrkontaktid.Id, Contact.Fields.FullName, Contact.Fields.AP_virksomhedsid, Contact.Fields.GovernmentId,
                                Contact.Fields.AP_fullname, Contact.Fields.Address1_Line1, Contact.Fields.Address1_PostalCode, Contact.Fields.Address1_City, Contact.Fields.Address1_Name);

                            this.Trace.LogInformation($"Brev til kontaktperson {kontaktPerson.FullName} cvr {kontaktPerson.AP_virksomhedsid} cpr {(kontaktPerson.GovernmentId != null && kontaktPerson.GovernmentId.Length >= 6 ? kontaktPerson.GovernmentId.Substring(0, 6) : "")} hvis TilladSendTilDigitalPost er tilladt {TilladSendTilDigitalPost}");

                            byte[] data = BrevOrienteringSletning(konto, kontaktPerson, ejendom, sagsnummer);

                            #region Send brev via e-boks
                            if (this.TilladSendTilDigitalPost == true)
                            {
                                try
                                {
                                    string webserviceurl = string.Empty;
                                    string certificat = string.Empty;
                                    string prefix = "dk.gi.digitalpost.";
                                    SortedList<string, string> config = CrmContext.HentConfigMedPraefiks(prefix);
                                    // First url
                                    if (config.ContainsKey(prefix + "webserviceurl") == true)
                                        webserviceurl = config[prefix + "webserviceurl"];
                                    else
                                        Trace.LogError($"Setting '{prefix}webserviceurl' blev ikke fundet i Config_configsetting");
                                    // Then Certificate
                                    if (config.ContainsKey(prefix + "certificatfil") == true)
                                        certificat = config[prefix + "certificatfil"].KrypteringFjern();
                                    else
                                        Trace.LogError($"Setting '{prefix}certificatfil' blev ikke fundet i Config_configsetting");

                                    Trace.LogInformation($"Url:{webserviceurl}");

                                    Digitalpost digitalpost = new Digitalpost(certificat, webserviceurl);

                                    this.Trace.LogInformation($"Vi sender brev til kontaktperson : {kontaktPerson.FullName} {kontoNr}");

                                    digitalpost.SendBrevDigitaltEllerSomPost(new Brev
                                    {
                                        Dokumenttype = DokumentTypeEnum.Almindeligt_Brev,
                                        Modtagertype = string.IsNullOrEmpty(kontaktPerson.AP_virksomhedsid) ? ModtagerTypeEnum.Cpr : ModtagerTypeEnum.Cvr,
                                        Modtager = string.IsNullOrEmpty(kontaktPerson.AP_virksomhedsid) ? kontaktPerson.GovernmentId : kontaktPerson.AP_virksomhedsid,
                                        Dokument = data,
                                        EboksTitel = "Vedligeholdelseskonto i GI",
                                    },
                                    null);
                                }
                                catch (Exception digitalpostFejl)
                                {
                                    throw new Exception($"Der blev dannet en sag på {kontoNr} {sagsnummer} som bør fjernes igen! Digitalpost fejlede : " + digitalpostFejl.Message);
                                }
                            }
                            else
                            {
                                this.Trace.LogInformation($"Program sender ikke digital post : {kontoNr}");
                            }
                            #endregion

                            // Opret opgave
                            Guid aktivitetsId = this.OpretOpgave(sagsnummer);

                            // Upload filer
                            this.UploadFiler(aktivitetsId, data);

                            // Luk sag/aktivitet
                            this.Trace.LogInformation($"Luk sag/aktivitet : {sagsnummer}");

                            // Vent 5 sekunder før vi lukker sag/aktivitet grundet pluging i CRM som ikke altid er klar
                            System.Threading.Thread.Sleep(5000);

                            LukSagOgRelateredeAktiviteterRequest lukSagOgRelateredeAktiviteterRequest = new LukSagOgRelateredeAktiviteterRequest(this.CrmContext)
                            {
                                Sagsnummer = sagsnummer,
                                Beskrivelse = ""
                            };
                            LukSagOgRelateredeAktiviteterResponse lukSagOgRelateredeAktiviteterResponse = lukSagOgRelateredeAktiviteterRequest.Execute<LukSagOgRelateredeAktiviteterResponse>();

                            if (!lukSagOgRelateredeAktiviteterResponse.Status.IsOK())
                            {
                                throw new Exception($"Sag/Aktivitet skal lukkes. Vi lukker ny sag/aktivitet mislykkedes : {sagsnummer}");
                            }

                            this.Trace.LogInformation($"Sætter konto i bero {kontoNr} {sagsnummer} færdigbehandlet.");

                            kontoManagerUpdate.Update(new AP_konto
                            {
                                Id = konto.Id,
                                AP_statusframapper = new OptionSetValue((int)AP_konto_AP_statusframapper.Bero)
                            });

                            this.Trace.LogInformation($"Der blev dannet en sag på {kontoNr} {sagsnummer} færdigbehandlet.");
                        }
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                this.Trace.LogError($"Sagen fejlede : {kontoNr} {exception.Message}");
                return false;
            }
        }

        string OpretSag(AP_konto konto)
        {
            this.Trace.LogInformation($"OpretSag konto : {konto.AP_Kontonr}  OpgaveAktivitetEmne {this.OpgaveAktivitetEmne} ");

            OpretSagRequest opretSagRequest = new OpretSagRequest(this.CrmContext)
            {
                Brugernavn = this.BrugerNavn,
                KundeIdentifikation = this.KundeId,
                Emne = this.SagsEmne,
                Beskrivelse = this.OpgaveAktivitetEmne,
                EjendeEntitet = OpretSagEjendeEntitet.Konto,
                EjendeEntitetIdentifikation = konto.AP_Kontonr,
                ForcerNySag = true,
                Type = this.SagsType
            };
            OpretSagResponse opretSagResponse = opretSagRequest.Execute<OpretSagResponse>();

            if (!opretSagResponse.Status.IsOK())
            {
                throw new Exception($"OpretSag fejl konto : {konto.AP_Kontonr} {opretSagResponse.Status.Message}");
            }

            return opretSagResponse.Sagsnummer;
        }

        Guid OpretOpgave(string sagnummer)
        {
            this.Trace.LogInformation($"OpretOpgave Sagnummer : {sagnummer} OpgaveAktivitetskode {this.OpgaveAktivitetskode} OpgaveAktivitetEmne {this.OpgaveAktivitetEmne} ");

            OpretOpgaveRequest opretOpgaveRequest = new OpretOpgaveRequest(this.CrmContext)
            {   
                Sagsnummer = sagnummer,
                Aktivitetskode = this.OpgaveAktivitetskode,
                Emne = this.OpgaveAktivitetEmne,
                Beskrivelse = this.OpgaveAktivitetEmne,
                LukOpgaven = false,
                AktivVedBrevfletning = true
            };
            OpretOpgaveResponse opretOpgaveResponse = opretOpgaveRequest.Execute<OpretOpgaveResponse>();

            if (!opretOpgaveResponse.Status.IsOK())
            {
                throw new Exception($"OpretOpgave sagnummer fejl : {sagnummer} {opretOpgaveResponse.Status.Message}");
            }

            return opretOpgaveResponse.Id;
        }

        void UploadFiler(Guid aktivitetsId, Byte[] brevOrienteringSletning)
        {
            this.Trace.LogInformation($"UploadFiler aktivitetsId : {aktivitetsId}");

            UploadFilRequest uploadFilRequest = new UploadFilRequest(this.CrmContext)
            {
                AktivitetsId = aktivitetsId,
                Beskrivelse = "Vedligeholdelseskonto i GI - orientering om sletning",
                FilIndhold = brevOrienteringSletning,
                Titel = "Vedligeholdelseskonto i GI - orientering om sletning",
                Filnavn = "Vedligeholdelseskonto i GI - orientering om sletning.pdf",
                Filtype = "application/pdf"
            };
            UploadFilResponse uploadFilResponse = uploadFilRequest.Execute<UploadFilResponse>();

            if (!uploadFilResponse.Status.IsOK())
            {
                throw new Exception($"UploadFiler fejl brevOrienteringSletning  {uploadFilResponse.Status.Message}");
            }
        }

        string LangDato_daDK(DateTime dato)
        {
            CultureInfo kultur = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("da-DK");
            string langDato = dato.ToLongDateString();
            Thread.CurrentThread.CurrentCulture = kultur;
            return langDato;
        }

        Byte[] BrevOrienteringSletning(AP_konto konto, Contact kontaktPerson, AP_ejendom ejendom, string sagsnummer)
        {
            Dictionary<string, object> fletteData = new Dictionary<string, object>();

            fletteData.Add("Navn1", kontaktPerson.AP_fullname);
            fletteData.Add("Navn2", kontaktPerson.Address1_Line1);

            if (string.IsNullOrEmpty(kontaktPerson.Address1_Name))
            {
                fletteData.Add("Navn3", kontaktPerson.Address1_PostalCode + " " + kontaktPerson.Address1_City);
                fletteData.Add("Navn4", "");
            }
            else
            {
                fletteData.Add("Navn3", kontaktPerson.Address1_Name);
                fletteData.Add("Navn4", kontaktPerson.Address1_PostalCode + " " + kontaktPerson.Address1_City);
            }

            fletteData.Add("Ejendom", ejendom.AP_samletadresse);
            fletteData.Add("SVARFRIST", LangDato_daDK(DateTime.Today.AddDays(this.SvarFrist)));

            fletteData.Add("SAGSNR", sagsnummer);
            fletteData.Add("KONTONR", konto.AP_Kontonr);

            pdf pdf = new pdf();
            List<DataSet> dataSets = new List<DataSet>();
            return pdf.DanBrev(fletteData, dataSets, "KontoAnnulleres.xml");
        }
    }
}