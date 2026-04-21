using dk.gi.bc.servicelink;
using dk.gi.crm;
using dk.gi.crm.managers.V2;
using dk.gi.crm.models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;

namespace dk.gi.app.konto.aarligrentetilskrvning
{
    /// <summary>
    /// Hjælpe klasse til at udføre opdateringer til BC med.
    /// </summary>
    public class BC
    {
        // Vi skal bruge kontosystem værdier til bogføring
        KontosystemVaerdier KontosystemVaerdier { get; set; }
        ILogger Trace { get; set; }

        public BC(CrmContext crmContext, ILogger trace)
        {
            // Vi sætter kontosystem værdier til bogføring
            Ap_KontoSystemManager kontoSystemManager = new Ap_KontoSystemManager(crmContext);
            this.KontosystemVaerdier = kontoSystemManager.Vaerdier();
            this.Trace = trace;
        }

        /// <summary>
        /// Opretter journallinjer og bogføre disse i BC. Afvikler et konto interval ad gangen. 
        /// </summary>
        public void BogfoerRente(CrmContext crmContext, int aar, List<KontoRenteLinje> kontiRenteLinjer, string kontoForIndestaaende, string renteKonto, AzureBCModel azureBC)
        {
            try
            {
                decimal totalRente = 0;

                totalRente = kontiRenteLinjer.Where(r => r.Rente > 0).Sum(r => r.Rente);

                List<JournalLineModel> journalLinjer = new List<JournalLineModel>();

                journalLinjer.Add(new JournalLineModel
                {
                    postingDate = new DateTime(aar, 12, 31),
                    accountType = "G/L Account",
                    accountNumber = kontoForIndestaaende,
                    amount = totalRente * -1,
                    description = "Rentetilskrivning"
                });

                journalLinjer.Add(new JournalLineModel
                {
                    postingDate = new DateTime(aar, 12, 31),
                    accountType = "G/L Account",
                    accountNumber = renteKonto,
                    amount = totalRente,
                    description = "Rentetilskrivning"
                });

                this.Bogfoer(crmContext, journalLinjer, azureBC);
            }
            catch (Exception exception)
            {
                Trace.LogError(exception.Message + (exception.InnerException != null ? exception.InnerException.Message : ""));
                throw exception;
            }
        }

        /// <summary>
        /// Bogfører i BC.
        /// </summary>
        /// <param name="crmContext">En <see cref="CrmContext"/>, der benyttes til at hente BC indstillinger fra CRM med.</param>
        /// <param name="journalLinjer">En <see cref="JournalLineModel"/>, der benyttes til BC.</param>
        public void Bogfoer(CrmContext crmContext, List<JournalLineModel> journalLinjer, AzureBCModel azureBC)
        {
            Trace.LogInformation($"Bogfoer i BC");

            BogfoerRequest bogfoerRequest = new BogfoerRequest(crmContext)
            {
                journalLinjer = journalLinjer,
                AzureBC = azureBC
            };
            BogfoerResponse bogfoerResponse = bogfoerRequest.Execute<BogfoerResponse>();

            if (bogfoerResponse.Status.IsError())
            {
                throw new Exception("Bogføring i BC mislykkedes!");
            }
        }

    }
}
