using System;
using System.Collections.Generic;
using dk.gi.app.contact.selskab.Application.Models;

namespace dk.gi.app.contact.selskab.Application.Abstractions
{
    public interface IContactSelskabScanClient : IDisposable
    {
        ContactSelskabExecutionSummary VerifyConnection();
        IReadOnlyCollection<ContactSelskabOwnerObservation> GetOwnerObservations(Guid? companyContactId);
        ResolvedServiceBusSettings ResolveServiceBusSettings();
    }
}
