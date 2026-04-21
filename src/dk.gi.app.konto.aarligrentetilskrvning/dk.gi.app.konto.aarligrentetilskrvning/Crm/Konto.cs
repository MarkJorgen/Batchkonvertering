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
    public class Konto
    {
        /// <summary>
        /// Henter konti der skal arbejdes på
        /// </summary>
        public List<KontoLinje> HentAlle(ILogger trace, CrmContext crmContext)
        {
            trace.LogInformation("Henter konti der skal behandles...");

            HentKontoAlleRequest_V2 hentKontoAlleRequest = new HentKontoAlleRequest_V2(crmContext)
            {
                KunAktiveKonti = false,
                Attributer = new string[] { AP_konto.Fields.Id, AP_konto.Fields.AP_Kontonr, AP_konto.Fields.AP_statusframapper, AP_konto.Fields.AP_indestendebelb }
            };
            HentKontoAlleResponse_V2 hentKontoAlleResponse = (HentKontoAlleResponse_V2)hentKontoAlleRequest.Execute();

            if (!hentKontoAlleResponse.Status.IsOK())
            {
                trace.LogError(hentKontoAlleResponse.Status.Message);
      
                throw new Exception("Konti kunne ikke hentes!");
            }

            if (hentKontoAlleResponse.konti == null)
            {
                throw new Exception("Konti var null!");
            }

            trace.LogInformation($"Fandt antal konti {hentKontoAlleResponse.konti.Count}");

            List<AP_konto> konti = hentKontoAlleResponse.konti.Where(k => k.AP_statusframapper.HasValue() && (k.AP_statusframapper.Value == (int)AP_konto_AP_statusframapper.Aktiv ||
                k.AP_statusframapper.Value == (int)AP_konto_AP_statusframapper.Bero)).OrderBy(k => k.AP_Kontonr).ToList();

            trace.LogInformation($"Sender svar HentKonti");

            List<KontoLinje> kontoLinjer = new List<KontoLinje>();

            foreach (AP_konto konto in konti)
            {
                kontoLinjer.Add(new KontoLinje
                {
                    KontoId = konto.Id,
                    KontoNr = konto.AP_Kontonr,
                    Kontoindestaaende = konto.AP_indestendebelb.GetValueOrDefault(0)
                });
            }

            return kontoLinjer;
        }
    }
}
