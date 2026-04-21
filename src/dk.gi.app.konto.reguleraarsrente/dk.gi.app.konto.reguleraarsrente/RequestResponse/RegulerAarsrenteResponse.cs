using System;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;

// Microsoft CRM SDK NameSpace, fjern kommentar fra disse linjer hvis det er et CRM Projekt
//using Microsoft.Xrm.Sdk;
//using Microsoft.Xrm.Sdk.Query;
//using Microsoft.Crm.Sdk.Messages;
//using Microsoft.Xrm.Sdk.Messages;

// GI namespace
using dk.gi;
using dk.gi.crm;
using dk.gi.crm.managers;
using dk.gi.crm.response;
//using dk.gi.crm.giproxy;
//using dk.gi.crm.managers.V2;

// Namespace til dit response objekt
namespace dk.gi.crm.app.konto.reguleraarsrente
{
    /// <summary>
    /// A template for use in creating new requests (Copy this file to a new file)
    /// </summary>
    public class RegulerAarsrenteResponse : CrmResponse // Hvis det er et CRM projekt
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="trace">Trace object to create breadcrump</param>
        public RegulerAarsrenteResponse() : base()
        {            
        }
    }
}