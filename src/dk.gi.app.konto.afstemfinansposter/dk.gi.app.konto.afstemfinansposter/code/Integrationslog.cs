using dk.gi.cpr.servicelink;
using dk.gi.crm.giproxy;
using dk.gi.crm.managers.V2;
using dk.gi.crm.models;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.crm.app.konto.afstemfinansposter
{
    public class Integrationslog
    {
        public Guid Opret(CrmContext crmContext, OptionSetValue status, OptionSetValue dataleverandoer, OptionSetValue integrationspartner, string navn, string importfilnavn, string filId)
        {
            // Vi starter integrationslog
            OpretIntegrationslogRequest opretIntegrationslogRequest = new OpretIntegrationslogRequest((crmContext))
            {
                Status = status,
                Dataleverandoer = dataleverandoer,
                Integrationspartner = integrationspartner,
                Navn = navn,
                Importfilnavn = importfilnavn,
                FilId = filId
            };
            OpretIntegrationslogResponse opretIntegrationslogResponse = opretIntegrationslogRequest.Execute<OpretIntegrationslogResponse>();

            if (!opretIntegrationslogResponse.Status.IsOK())
            {
                throw new Exception("Integrationslog opret fejlede");
            }

            // Vi gemmer id til status opdatering
            return opretIntegrationslogResponse.Id;
        }

        public void OpdaterStatus(CrmContext crmContext, Guid id, OptionSetValue status, string integrationslog_til, string integrationslog_fra)
        {
            string[] til = integrationslog_til.Split(';');

            // Vi opdatere integrationslog med status
            OpdaterStatusIntegrationslogRequest opdaterStatusIntegrationslogRequest = new OpdaterStatusIntegrationslogRequest((crmContext))
            {
                Status = status,
                Id = id,
                Til = til,
                Afsender = integrationslog_fra
            };
            OpdaterStatusIntegrationslogResponse opdaterStatusIntegrationslogResponse = opdaterStatusIntegrationslogRequest.Execute<OpdaterStatusIntegrationslogResponse>();

            if (!opdaterStatusIntegrationslogResponse.Status.IsOK())
            {
                throw new Exception("Integrationslog opdater status fejlede");
            }
        }

        public void OpdaterNavnMedSaldi(CrmContext crmContext, Guid id, AfstemningSum afstemningSum, string integrationsNavn)
        {
            using (IntegrationslogManager integrationslogManager = new IntegrationslogManager(crmContext))
            {
                integrationslogManager.OpdaterNavn(id, $"{integrationsNavn} crm : {afstemningSum.CRMDatoSaldo.Saldo.ToString("N2", CultureInfo.CreateSpecificCulture("da-DK"))}  økonomi : {afstemningSum.OekonomiDatoSaldo.Saldo.ToString("N2", CultureInfo.CreateSpecificCulture("da-DK"))}");
            }
        }

        public DateTime SidsteOKOpdateringsDato(CrmContext crmContext)
        {
            using (IntegrationslogManager integrationslogManager = new IntegrationslogManager(crmContext))
            {
                var data = integrationslogManager.HentNyeste("%Afstemning af finansposter%", new OptionSetValue((int)ap_integrationslog_ap_dataleverandoer.GI),
                    new OptionSetValue((int)ap_integrationslog_ap_status.AfsluttetOKCRM), ap_integrationslog.Fields.ap_name, ap_integrationslog.Fields.CreatedOn);
        
                if(data == null)
                {
                    throw new Exception("Integrationslog SidsteOKOpdateringsDato fejlede");
                }

                return data.CreatedOn.Value.Date;
            }
        }
    }
}
