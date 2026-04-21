using System;

namespace dk.gi.app.contact.registrering.optaelling.Application.Models
{
    public sealed class ContactRegistreringOptaellingRequest
    {
        public Guid? RegistreringId { get; }
        public string Mode { get; }

        public ContactRegistreringOptaellingRequest(Guid? registreringId, string mode)
        {
            RegistreringId = registreringId;
            Mode = mode ?? string.Empty;
        }
    }
}
