using System;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;

// CRM SDK NameSpace
// Microsoft CRM SDK
//using Microsoft.Xrm.Sdk;
//using Microsoft.Xrm.Sdk.Query;
//using Microsoft.Crm.Sdk.Messages;
//using Microsoft.Xrm.Sdk.Messages;

// GI namespace
using dk.gi;
using dk.gi.crm;
using dk.gi.crm.managers;
using dk.gi.crm.response;
using Microsoft.Extensions.Logging;
//using dk.gi.crm.giproxy;
//using dk.gi.crm.managers.V2;

// Namespace til dit request objekt
namespace dk.gi.crm.app.konto.reguleraarsrente
{
    /// <summary>
    /// A template for use in creating new requests (Copy this file to a new file)
    /// 
    /// Termplate inherits from IRequest, GIRequest or CrmRequest
    /// - You may/can not create a new request with a constructor without parameters, at lest a Trace objekt for tracking
    /// - please create a constructor without parameters and then set it to Obsolete to prevent unwanted use!
    /// </summary>
    public class RegulerAarsrenteRequest : CrmRequest // If this is a CRM request
    {
        public RegulerAarsrenteRequest(CrmContext context) : base(context) { }  // If this is a CRM request

        /// <summary>
        /// Funktionen ExecuteRequest indeholder den kode der skal udføres.
        /// </summary>
        /// <returns>Et response som altid indeholder en Status på hvordan udførsel gik</returns>
        protected override IResponse ExecuteRequest()
        {
            RegulerAarsrenteResponse result = new RegulerAarsrenteResponse();

            if (result.Status.IsOK() == false)
                return result;

            // Validering is done
            Trace.LogInformation($"Validereing af egenskaber i request {GetType().Name}, blev fuldført uden fejl.");
            try
            {
                using (dk.gi.crm.managers.V2.ap_finanssaldoManager managerfinanssaldo = new dk.gi.crm.managers.V2.ap_finanssaldoManager(this.localCrmContext))
                {
                    Trace.LogInformation("OpdaterNegativArsrente");
                    managerfinanssaldo.OpdaterNegativArsrente(DateTime.Now.Year);
                }

                // Information to trace, code completed this method without exceptions
                Trace.LogInformation($"Request {GetType().Name} blev gennemført");
            }
            catch (Exception ex)
            {
                result.Status.AppendError($"Der opstod en Exception i {GetType().Name}", ex);
            }

            // This is the output of the work done in this request
            return result;
        }
    }
}