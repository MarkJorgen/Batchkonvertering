using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using dk.gi.crm;


// GI
using dk.gi.crm.giproxy;
using dk.gi.crm.managers.V2;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace dk.gi.app.konto.aarligrentetilskrvning
{
    /// <summary>
    /// En forespørgsel til at hente konti
    /// </summary>
    /// <remarks>
    /// Oprettet af RCL, den 2018 11 26
    /// </remarks>
    public class KontiRequest : CrmRequest
    {
        #region .ctor
        /// <summary>
        /// 
        /// </summary>
        public KontiRequest(CrmContext context) : base(context) { }
        #endregion

        #region properties
        #endregion

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override IResponse ExecuteRequest()
        {
            KontiResponse result = new KontiResponse();

            try
            {
                using (Ap_KontoManager managerKonto = new Ap_KontoManager(this.localCrmContext))
                {
                    List<AP_konto> konti = new List<AP_konto>();

                    konti.AddRange(managerKonto.HentAlle(true, AP_konto.Fields.Id, AP_konto.Fields.AP_Kontonr).ToList().OrderBy(k => k.AP_Kontonr));

                    result.konti = konti;
                }
            }
            catch (Exception exception)
            {
                // Log fejlen
                Trace.LogError(exception.Message);
                throw exception;
            }

            return result;
        }
    }

    public class KontiResponse : CrmResponse
    {
        public KontiResponse() : base() { }

        public List<AP_konto> konti { get; set; } = new List<AP_konto>();
    }

}