using System;

namespace dk.gi.app.contact.lassox.ophoer.Application.Models
{
    public sealed class LassoXOphoerDecision
    {
        public Guid ContactId { get; }
        public string FullName { get; }
        public bool KeepSubscription { get; }
        public string Reason { get; }

        public LassoXOphoerDecision(Guid contactId, string fullName, bool keepSubscription, string reason)
        {
            ContactId = contactId;
            FullName = fullName ?? string.Empty;
            KeepSubscription = keepSubscription;
            Reason = reason ?? string.Empty;
        }
    }
}
