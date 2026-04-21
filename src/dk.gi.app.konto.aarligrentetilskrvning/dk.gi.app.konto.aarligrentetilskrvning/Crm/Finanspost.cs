using dk.gi.crm;
using dk.gi.crm.giproxy;
using dk.gi.crm.managers.V2;
using dk.gi.crm.models;
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
    public class Finanspost
    {
        /// <summary>
        /// Opretter finanspost i CRM
        /// </summary>
        public void Opret(ILogger trace, ap_finansposteringManager finansposteringManager, KontoRenteLinje kontoRenteLinje, string udbetalingsKode, string finanspostTekst, int aar)
        {
            trace.LogInformation($"Opretter finanspost for {kontoRenteLinje.KontoNr} {udbetalingsKode} {kontoRenteLinje.Rente} {finanspostTekst} i CRM...");

            finansposteringManager.OpretFinanspostering(new Finanspostering
            {
                KontoId = kontoRenteLinje.KontoId,
                Beloeb = kontoRenteLinje.Rente,
                Tekst = finanspostTekst,
                Udbetalingskode = udbetalingsKode,
                BogfoeringsDato = new DateTime(aar, 12, 31),
                PosteringsDato = new DateTime(aar, 12, 31),
                ValoerDato = new DateTime(aar, 12, 31),
                Kilde = (int)ap_finanspostering_ap_kilde.CRMBatch
            });
        }
    }
}
