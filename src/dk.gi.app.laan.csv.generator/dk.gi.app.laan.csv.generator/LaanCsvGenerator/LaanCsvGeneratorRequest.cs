using dk.gi.cpr.servicelink;
using dk.gi.crm.managers;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using dk.gi.email;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Web.Mail;

namespace dk.gi.crm.app.LaanCsvGenerator
{
    /// <summary>
    /// ArealTjekRequest 
    /// </summary>
    public class LaanCsvGeneratorRequest : CrmRequest
    {
        public LaanCsvGeneratorRequest(CrmContext context) : base(context) { }

        /// <summary>
        /// EmailClientId
        /// </summary>
        [DataMember(IsRequired = true)]
        public string EmailClientId { get; set; }

        /// <summary>
        /// EmailClientSecret
        /// </summary>
        [DataMember(IsRequired = true)]
        public string EmailClientSecret { get; set; }

        /// <summary>
        /// EmailTenantid
        /// </summary>
        [DataMember(IsRequired = true)]
        public string EmailTenantid { get; set; }

        /// <summary>
        /// EmailAfsenderMailAdressse
        /// </summary>
        [DataMember(IsRequired = true)]
        public string EmailAfsenderMailAdressse { get; set; }


        /// <summary>
        /// Funktionen ExecuteRequest indeholder den kode der skal udføres.
        /// </summary>
        /// <returns>Et response som altid indeholder en Status på hvordan udførsel gik</returns>
        protected override IResponse ExecuteRequest()
        {
            // Ret GenericStrignResponse til dit eget response 
            LaanCsvGeneratorResponse result = new LaanCsvGeneratorResponse();

            try
            {
                this.Trace.LogInformation("Har opsat tracing, opretter et CRM data proxy objekt.");

                StringBuilder filAftale = new StringBuilder();
                StringBuilder filAftaleProfil = new StringBuilder();

                // 2016-10-11 RMP: Opret vores forespørgsel til forretningslaget
                var forespoergsel = new HentAndelsboliglaanSkatRequest(this.localCrmContext)
                {
                    AntalAarPerLaaneProfil = 10,
                    Opgoerelsesdato = new DateTime(DateTime.Now.Year - 1, 12, 31)
                };

                // 2016-10-11 RMP: Hent data via vores forretningslag
                var svar = forespoergsel.Execute<HentAndelsboliglaanSkatResponse>();

                // Fandt vi et eller flere andelsboliglån
                if (svar.Laan != null && svar.Laan.Any())
                {
                    //
                    this.Trace.LogInformation($"Har hentet alle lån for andelsboligforeninger, der er {svar.Laan.Length} styks");

                    // 2014-09-16 RMP: Opret et serialiserings-objekt for lånene
                    var cs = new CsvSerializer<Laan>();
                    //
                    this.Trace.LogInformation("CsvSerializer er oprettet...");

                    // 2016-10-11 RMP: Konvertering fra response.HentAndelsboliglaanSkatLaan til Laan
                    var loans = svar.Laan
                        .Select(l => new Laan
                        {
                            LåneNummer = l.LaaneNr,
                            AfdragPaabegyndes = l.AfdragPaabegyndes,
                            CVRNR = l.CvrNummer,
                            Restgæld = l.Restgaeld,
                            RestLoebetid = l.RestLoebetid,
                        })
                        .ToArray();

                    // 2014-09-16 RMP: Generate the CSV content for the first file Aftale_2025-01-13
                    filAftale.Append(cs.Generate(loans));
                }

                // Fandt vi en eller flere låneprofiler
                if (svar.LaaneProfiler != null && svar.LaaneProfiler.Any())
                {
                    //
                    this.Trace.LogInformation($"Har hentet alle låneprofiler for lån til andelsboligforeninger, der er {svar.LaaneProfiler.Length} styks");

                    // 2014-09-16 RMP: Opret et serialiserings-objekt for låneprofilerne
                    var loanProfileCsvSerializer = new CsvSerializer<LaanProfile>();
                    //
                    this.Trace.LogInformation("CsvSerializer er oprettet...");

                    // 2016-10-11 RMP: Konvertering fra response.HentAndelsboliglaanSkatLaanProfil til LaanProfile
                    var loanProfiles = svar.LaaneProfiler
                        .Select(l => new LaanProfile
                        {
                            AftaleNummer = ((l.LaaneNr) + "       ").Left(7),
                            År = l.Aar,
                            NominelRestgæld = l.NominelRestgaeld,
                            ÅrligYdelse = l.AarligYdelse,
                        })
                        .ToArray();
                    //
                    this.Trace.LogInformation("Har konverteret alle låneprofilerne for lån til andelsboligforeninger, fra HentAndelsboliglaanSkatLaanProfil objekter til LaanProfile objekter");

                    // 2014-09-16 RMP: Generate the CSV content for the first file AftaleProfil_2025-01-13
                    filAftaleProfil.Append(loanProfileCsvSerializer.Generate(loanProfiles));
                }

                using (ConfigurationSettingsManager configManager = new ConfigurationSettingsManager(this.localCrmContext))
                {
                    Trace.LogInformation("app.konto.indberetskat.email.modtager");

                    string emailModtager = configManager.Hent("app.laan.csv.generator.email.modtager");

                    string[] EmailModtagere = new string[] { emailModtager };
                    string CrmServerName = this.localCrmContext.GetCrmServerName();

                    List<dk.gi.email.EmailAttachment> emailAttachment = new List<dk.gi.email.EmailAttachment>();

                    Encoding enc = Encoding.GetEncoding(1252);

                    string filDataBase641 = Convert.ToBase64String(enc.GetBytes(filAftale.ToString()));
                    emailAttachment.Add(new EmailAttachment { Name = $"Aftale{DateTime.Now.Year}_{DateTime.Now.Month.ToString("D2")}_{DateTime.Now.Day.ToString("D2")}.csv", ContentType = "text/plain", IndholdBase64 = filDataBase641 });

                    string filDataBase642 = Convert.ToBase64String(enc.GetBytes(filAftaleProfil.ToString()));
                    emailAttachment.Add(new EmailAttachment { Name = $"AftaleProfil{DateTime.Now.Year}_{DateTime.Now.Month.ToString("D2")}_{DateTime.Now.Day.ToString("D2")}.csv", ContentType = "text/plain", IndholdBase64 = filDataBase642 });

                    // Set Email content
                    string subject = $"dk.gi.app.laan.csv.generator, Crm:{CrmServerName}, Dato:{System.DateTime.Now.ToString("yyyy-MM-dd hh:mm")}";
                    string body = $"Se vedhæftede filer laan.csv.generator kørsel.\r\n\r\nDette er en automatisk genereret mail, fra {CrmServerName}.\r\n";

                    dk.gi.email.EmailContext eContext = new dk.gi.email.EmailContext(this.EmailClientId, this.EmailClientSecret,  this.EmailTenantid, this.EmailAfsenderMailAdressse);

                    // Send mail
                    if (dk.gi.email.EmailClient.SendEmail(eContext, subject, body, EmailModtagere, false, emailAttachment) == false)
                    {
                        Trace.LogError("Kunne ikke sende mail");
                        throw new Exception("Kunne ikke sende mail");
                    }
                }
            }
            catch (Exception exception)
            {
                string fejl = exception.Message;
                this.localCrmContext.Trace.LogError(fejl);
                result.Status.AppendError(fejl);
            }

            if (result.Status.IsOK())
            {
                // Information to trace, code completed this method without exceptions
                Trace.LogInformation($"Request {GetType().Name} blev gennemført");
            }

            return result;
        }
    }
}