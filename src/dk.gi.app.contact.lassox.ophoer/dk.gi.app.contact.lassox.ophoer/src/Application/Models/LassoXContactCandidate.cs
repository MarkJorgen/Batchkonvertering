using System;

namespace dk.gi.app.contact.lassox.ophoer.Application.Models
{
    public sealed class LassoXContactCandidate
    {
        public Guid ContactId { get; }
        public string FullName { get; }

        public LassoXContactCandidate(Guid contactId, string fullName)
        {
            ContactId = contactId;
            FullName = fullName ?? string.Empty;
        }
    }
}
