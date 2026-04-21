using System;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Config
{
    public static class ContactRegistreringUdloebneOptaellingRequestFactory
    {
        public static ContactRegistreringUdloebneOptaellingRequest Create(JobConfiguration configuration, ContactRegistreringUdloebneOptaellingSettings settings)
        {
            Guid? registreringId = null;
            if (configuration.TryGet("registreringid", out var rawValue) && string.IsNullOrWhiteSpace(rawValue) == false)
            {
                if (!Guid.TryParse(rawValue, out var parsed))
                {
                    throw new InvalidOperationException("registreringid er ikke en gyldig Guid.");
                }

                registreringId = parsed;
            }

            return new ContactRegistreringUdloebneOptaellingRequest(registreringId, settings.Mode);
        }
    }
}
