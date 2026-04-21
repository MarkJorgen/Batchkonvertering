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
using dk.gi.crm.request;
using dk.gi.crm.response;
using Microsoft.Extensions.Logging;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using dk.gi.crm.managers.V2;
using dk.gi.crm.giproxy;
using System.Data;
using System.Globalization;
using dk.gi.crm.models;
using System.IO;
using dk.gi.asbq;
using dk.gi.lassox.servicelink;
//using dk.gi.crm.giproxy;
//using dk.gi.crm.managers.V2;

// Namespace til dit request objekt
namespace dk.gi.crm.app.konto.indberet.regnskab
{
    /// <summary>
    /// A template for use in creating new requests (Copy this file to a new file)
    /// 
    /// Termplate inherits from IRequest, GIRequest or CrmRequest
    /// - You may/can not create a new request with a constructor without parameters, at lest a Trace objekt for tracking
    /// - please create a constructor without parameters and then set it to Obsolete to prevent unwanted use!
    /// </summary>
    public class IndberetRegnskabRequest : CrmRequest // If this is a CRM request
    {
        public IndberetRegnskabRequest(CrmContext context) : base(context) { }  // If this is a CRM request

        /// <summary>
        /// Funktionen ExecuteRequest indeholder den kode der skal udføres.
        /// </summary>
        /// <returns>Et response som altid indeholder en Status på hvordan udførsel gik</returns>
        protected override IResponse ExecuteRequest()
        {
            IndberetRegnskabResponse result = new IndberetRegnskabResponse();

            if (result.Status.IsOK() == false)
                return result;

            // Validering is done
            Trace.LogInformation($"Validereing af egenskaber i request {GetType().Name}, blev fuldført uden fejl.");
            try
            {
                // Information to trace, code completed this method without exceptions
                Trace.LogInformation($"Request {GetType().Name} blev gennemført");

                int regnskabStartmaaned = this.RegnskabStartmaaned(DateTime.Today);
                this.Trace.LogInformation($"regnskabStartmaaned {regnskabStartmaaned}");

                DateTime sidsteRegnskabsdato = this.SidsteRegnskabsdato(DateTime.Today);
                this.Trace.LogInformation($"sidsteRegnskabsdato {sidsteRegnskabsdato}");

                // 2022 03 14 RCL Hent indberet regnskab kontakter
                HentIndberetRegnskabsFristRequest hentIndberetRegnskabsFristRequest = new HentIndberetRegnskabsFristRequest(this.localCrmContext)
                {
                    RegnskabStartmaaned = regnskabStartmaaned,
                    SidsteRegnskabsdato = sidsteRegnskabsdato
                };
                HentIndberetRegnskabsFristResponse hentIndberetRegnskabsFristResponse = hentIndberetRegnskabsFristRequest.Execute<HentIndberetRegnskabsFristResponse>();

            }
            catch (Exception ex)
            {
                result.Status.AppendError($"Der opstod en Exception i {GetType().Name}", ex);
            }

            // This is the output of the work done in this request
            return result;
        }

        int RegnskabStartmaaned(DateTime dato)
        {
            switch (dato.Month)
            {
                case 4:
                    return 1;
                case 5:
                    return 2;
                case 6:
                    return 3;
                case 7:
                    return 4;
                case 8:
                    return 5;
                case 9:
                    return 6;
                case 10:
                    return 7;
                case 11:
                    return 8;
                case 12:
                    return 9;
                case 1:
                    return 10;
                case 2:
                    return 11;
                case 3:
                    return 12;
            }

            throw new Exception("Start måned ikke fundet!");
        }

        DateTime SidsteRegnskabsdato(DateTime dato)
        {
            // 2022 03 14 RCL
            // April ap_regnskabstartmned=1
            // 1-(1)-2023 - 2 dage giver ap_sidsteregnskabsdato=30-12-2022

            int aar = dato.Year;

            if (dato.Month == 1 || dato.Month == 2 || dato.Month == 3)
            {
                aar = aar - 1;
            }

            int maaned = RegnskabStartmaaned(dato);

            DateTime sidsteRegnskabsdato = new DateTime(aar, maaned, 1).AddDays(-2);
            return sidsteRegnskabsdato;
        }
    }
}