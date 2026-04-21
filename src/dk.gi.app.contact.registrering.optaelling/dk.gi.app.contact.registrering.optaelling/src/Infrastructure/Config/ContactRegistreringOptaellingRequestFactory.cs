using System;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.contact.registrering.optaelling.Infrastructure.Config
{
    public static class ContactRegistreringOptaellingRequestFactory
    {
        public static ContactRegistreringOptaellingRequest Create(JobConfiguration configuration, ContactRegistreringOptaellingSettings settings)
        {
            Guid? registreringId = null;
            if (configuration.TryGet("registreringid", out var rawValue) && string.IsNullOrWhiteSpace(rawValue) == false)
            {
                if (Guid.TryParse(rawValue, out var parsed) == false)
                {
                    throw new InvalidOperationException("registreringid er ikke en gyldig Guid.");
                }

                registreringId = parsed;
            }

            return new ContactRegistreringOptaellingRequest(registreringId, settings.Mode);
        }
    }
}
