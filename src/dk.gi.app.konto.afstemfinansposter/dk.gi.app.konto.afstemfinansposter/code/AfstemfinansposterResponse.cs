using System;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;
// GI namespace
using dk.gi;
using dk.gi.crm;
using dk.gi.crm.managers;
using dk.gi.crm.request;
using dk.gi.crm.response;
//using dk.gi.crm.giproxy;
//using dk.gi.crm.managers.V2;

// Namespace til dit response objekt
namespace dk.gi.crm.response.V2
{
    /// <summary>
    /// A template for use in creating new requests (Copy this file to a new file)
    /// </summary>
    public class AfstemfinansposterResponse : CrmResponse // Hvis det er et CRM projekt
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="trace">Trace object to create breadcrump</param>
        public AfstemfinansposterResponse() : base()
        {            
        }

    }
}