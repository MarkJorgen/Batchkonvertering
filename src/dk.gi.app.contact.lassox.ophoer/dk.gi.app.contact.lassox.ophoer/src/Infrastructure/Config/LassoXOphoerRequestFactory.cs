using System;
using dk.gi.app.contact.lassox.ophoer.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.contact.lassox.ophoer.Infrastructure.Config
{
    public static class LassoXOphoerRequestFactory
    {
        public static LassoXOphoerRequest Create(JobConfiguration configuration, LassoXOphoerSettings settings)
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

            return new LassoXOphoerRequest(contactId, settings.Mode, settings.RunMode);
        }
    }
}
