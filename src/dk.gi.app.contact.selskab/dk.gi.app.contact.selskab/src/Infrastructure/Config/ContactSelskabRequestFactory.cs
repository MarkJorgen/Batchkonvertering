using System;
using dk.gi.app.contact.selskab.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.contact.selskab.Infrastructure.Config
{
    public static class ContactSelskabRequestFactory
    {
        public static ContactSelskabRequest Create(JobConfiguration configuration, ContactSelskabSettings settings)
        {
            Guid? contactId = null;
            if (configuration.TryGet("contactid", out var rawValue) && string.IsNullOrWhiteSpace(rawValue) == false)
            {
                if (!Guid.TryParse(rawValue, out var parsed))
                {
                    throw new InvalidOperationException("contactid er ikke en gyldig Guid.");
                }

                contactId = parsed;
            }

            return new ContactSelskabRequest(contactId, settings.Mode, settings.RunMode);
        }
    }
}
