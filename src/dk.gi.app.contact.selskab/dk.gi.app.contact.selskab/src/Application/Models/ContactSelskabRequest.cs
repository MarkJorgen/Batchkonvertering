using System;

namespace dk.gi.app.contact.selskab.Application.Models
{
    public sealed class ContactSelskabRequest
    {
        public Guid? ContactId { get; }
        public string Mode { get; }
        public bool RunMode { get; }

        public ContactSelskabRequest(Guid? contactId, string mode, bool runMode)
        {
            ContactId = contactId;
            Mode = mode ?? string.Empty;
            RunMode = runMode;
        }
    }
}
