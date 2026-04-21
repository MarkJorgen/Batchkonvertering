using dk.gi.crm;
using dk.gi.crm.giproxy;
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
    public class Finanssaldo
    {
        /// <summary>
        /// Henter finanssaldi der skal arbejdes på
        /// </summary>
        public List<FinanssaldoLinje> HentAlle(ILogger trace, CrmContext crmContext, int aar)
        {
            trace.LogInformation($"Henter finansaldi der skal behandles for år {aar.ToString()}...");

            HentFinanssaldoTilAarRequest_V2 hentFinanssaldoTilAarRequest = new HentFinanssaldoTilAarRequest_V2(crmContext)
            {
                Aar = aar,
                Attributer = new string[] { ap_finanssaldo.Fields.Id, ap_finanssaldo.Fields.ap_kontoId, ap_finanssaldo.Fields.ap_aarsrente, ap_finanssaldo.Fields.ap_primodato }
            };
            HentFinanssaldoTilAarResponse_V2 hentFinanssaldoTilAarResponse = (HentFinanssaldoTilAarResponse_V2)hentFinanssaldoTilAarRequest.Execute();

            if (!hentFinanssaldoTilAarResponse.Status.IsOK())
            {
                throw new Exception("Finanssaldi kunne ikke hentes!");
            }

            List<FinanssaldoLinje> finanssaldoLinjer = new List<FinanssaldoLinje>();

            foreach (ap_finanssaldo finanssaldo in hentFinanssaldoTilAarResponse.finanssaldi)
            {
                finanssaldoLinjer.Add(
                    new FinanssaldoLinje
                    {
                        Id = finanssaldo.Id,
                        KontoId = finanssaldo.ap_kontoId.Id,
                        AarsRente = finanssaldo.ap_aarsrente.HasValue() ? finanssaldo.ap_aarsrente.Value : 0
                    });
            }

            return finanssaldoLinjer;
        }

        /// <summary>
        /// Henter års rente for en konto ad gangen. 
        /// </summary>
        public KontoRenteLinje AarsRente(ILogger trace, KontoLinje kontoLinje, List<FinanssaldoLinje> finanssaldi)
        {
            string kontoNr = string.Empty;
            
            try
            {
                kontoNr = kontoLinje.KontoNr;

                KontoRenteLinje kontoRenteLinje = null;

                FinanssaldoLinje finanssaldo = finanssaldi.Where(f => f.KontoId != null && f.KontoId == kontoLinje.KontoId).FirstOrDefault();

                decimal rente = 0;

                // 2020 01 30 RCL Kun hvis kontoindestående > 0 kan vi give rente efter aftale med HRL 
                if (finanssaldo != null && (kontoLinje.Kontoindestaaende > 0))
                {
                    // Er finanssaldo.ap_aarsrente positiv er der rente
                    rente = (finanssaldo != null && finanssaldo.AarsRente > 0) ? finanssaldo.AarsRente : 0;
                    rente = decimal.Round(rente, 2);
                }
                else
                {
                    rente = 0;
                }

                // 2022 09 22 RCL only interest if > 0.05m
                if (rente > 0.05m)
                {
                    kontoRenteLinje = new KontoRenteLinje();
                    kontoRenteLinje.KontoNr = kontoLinje.KontoNr;
                    kontoRenteLinje.KontoId = kontoLinje.KontoId;
                    kontoRenteLinje.Rente = rente;
                }

                return kontoRenteLinje;
            }
            catch (Exception exception)
            {
                trace.LogError($"KontoRenteLinje AarsRente fejlede for {kontoNr} " + exception.Message + (exception.InnerException != null ? exception.InnerException.Message : ""));
                throw exception;
            }
        }
    }
}
