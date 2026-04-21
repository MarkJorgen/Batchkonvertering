using System;
using System.Collections.Generic;
using dk.gi.app.contact.lassox.ophoer.Application.Models;

namespace dk.gi.app.contact.lassox.ophoer.Application.Abstractions
{
    public interface ILassoXOphoerScanClient : IDisposable
    {
        LassoXOphoerExecutionSummary VerifyConnection();
        IReadOnlyCollection<LassoXContactCandidate> GetContactsWithActiveSubscription(Guid? contactId);
        IReadOnlyCollection<Guid> GetOpenAccountOwnerContactIds();
        IReadOnlyCollection<Guid> GetRealOwnerContactIds();
        int UnsubscribeContacts(IReadOnlyCollection<Guid> contactIds);
    }
}
