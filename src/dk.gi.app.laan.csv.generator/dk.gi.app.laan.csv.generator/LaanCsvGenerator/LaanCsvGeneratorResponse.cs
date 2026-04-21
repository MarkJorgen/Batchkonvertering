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
using dk.gi.crm.request;
using dk.gi.crm.response;

namespace dk.gi.crm.app.LaanCsvGenerator
{
    /// <summary>
    /// A template for use in creating new requests (Copy this file to a new file)
    /// </summary>
    // public class TemplateResponse : GIResponse // Hvis det IKKE er et CRM Projekt
    public class LaanCsvGeneratorResponse : CrmResponse // Hvis det er et CRM projekt
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public LaanCsvGeneratorResponse() : base()
        {            
        }

    }
}