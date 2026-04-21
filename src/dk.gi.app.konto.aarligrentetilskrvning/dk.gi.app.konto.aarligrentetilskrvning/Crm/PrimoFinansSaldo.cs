using dk.gi.crm;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.app.konto.aarligrentetilskrvning
{
    public class PrimoFinansSaldo
    {
        /// <summary>
        /// Opretter primofinanssaldo i CRM
        /// </summary>
        public void Opret(ILogger trace, CrmContext crmContext, Guid kontoId, string kontoNr, int aar)
        {
            trace.LogInformation($"Opretter primofinanssaldi for {kontoNr} i CRM...");

            List<Guid> kontoIds = new List<Guid>
            {
                kontoId
            };

            OpdaterFinanssaldoKontiRequest opdaterFinanssaldoKontiRequest = new OpdaterFinanssaldoKontiRequest(crmContext)
            {
                KontoIds = kontoIds,
                Primodato = new DateTime(aar + 1, 1, 1)
            };
            OpdaterFinanssaldoKontiResponse opdaterFinanssaldoKontiResponse = opdaterFinanssaldoKontiRequest.Execute<OpdaterFinanssaldoKontiResponse>();

            if (!opdaterFinanssaldoKontiResponse.Status.IsOK())
            {
                trace.LogError($"Oprettelse af primofinans saldo mislykkedes for konto {kontoNr}");
                throw new Exception($"Oprettelse af primofinans saldo mislykkedes for konto {kontoNr}");
            }
        }
    }
}
