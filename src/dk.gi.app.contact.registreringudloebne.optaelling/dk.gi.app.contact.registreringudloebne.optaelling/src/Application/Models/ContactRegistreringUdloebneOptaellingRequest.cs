using System;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Application.Models
{
    public sealed class ContactRegistreringUdloebneOptaellingRequest
    {
        public Guid? RegistreringId { get; }
        public string Mode { get; }

        public ContactRegistreringUdloebneOptaellingRequest(Guid? registreringId, string mode)
        {
            RegistreringId = registreringId;
            Mode = mode ?? string.Empty;
        }
    }
}
