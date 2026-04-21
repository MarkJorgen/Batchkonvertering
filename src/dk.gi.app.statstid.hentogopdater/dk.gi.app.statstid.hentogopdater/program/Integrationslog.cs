using dk.gi.crm;
using dk.gi.crm.giproxy;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.crm.app.statstid.hentogopdater
{
    public class Integrationslog
    {
        public Guid Opret(CrmContext crmContext, OptionSetValue status, OptionSetValue dataleverandoer, OptionSetValue integrationspartner)
        {
            DateTime dateTime = DateTime.Now;

            // Vi starter integrationslog
            OpretIntegrationslogRequest opretIntegrationslogRequest = new OpretIntegrationslogRequest((crmContext))
            {
                Status = status,
                Dataleverandoer = dataleverandoer,
                Integrationspartner = integrationspartner,
                Navn = "Statstidende",
                Importfilnavn = dateTime.ToShortDateString() + " " + dateTime.ToShortTimeString(),
                FilId = dateTime.ToShortDateString() + " " + dateTime.ToShortTimeString()
            };
            OpretIntegrationslogResponse opretIntegrationslogResponse = opretIntegrationslogRequest.Execute<OpretIntegrationslogResponse>();

            if (!opretIntegrationslogResponse.Status.IsOK())
            {
                throw new Exception("Integrationslog opret fejlede");
            }

            // Vi gemmer id til status opdatering
            return opretIntegrationslogResponse.Id;
        }

        public void OpdaterStatus(CrmContext crmContext, Guid id, OptionSetValue status)
        {
            // Vi opdatere integrationslog med status
            OpdaterStatusIntegrationslogRequest opdaterStatusIntegrationslogRequest = new OpdaterStatusIntegrationslogRequest((crmContext))
            {
                Status = status,
                Id = id,
                Til = new string[] { ConfigurationManager.AppSettings["Integrationslog_til"] },
                Afsender = ConfigurationManager.AppSettings["Integrationslog_fra"]
            };
            OpdaterStatusIntegrationslogResponse opdaterStatusIntegrationslogResponse = opdaterStatusIntegrationslogRequest.Execute<OpdaterStatusIntegrationslogResponse>();

            if (!opdaterStatusIntegrationslogResponse.Status.IsOK())
            {
                throw new Exception("Integrationslog opdater status fejlede");
            }
        }

    }
}
