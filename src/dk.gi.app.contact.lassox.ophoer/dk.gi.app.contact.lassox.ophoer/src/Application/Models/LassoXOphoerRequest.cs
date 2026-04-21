using System;

namespace dk.gi.app.contact.lassox.ophoer.Application.Models
{
    public sealed class LassoXOphoerRequest
    {
        public Guid? ContactId { get; }
        public string Mode { get; }
        public bool WriteChanges { get; }

        public LassoXOphoerRequest(Guid? contactId, string mode, bool writeChanges)
        {
            ContactId = contactId;
            Mode = mode ?? string.Empty;
            WriteChanges = writeChanges;
        }
    }
}
