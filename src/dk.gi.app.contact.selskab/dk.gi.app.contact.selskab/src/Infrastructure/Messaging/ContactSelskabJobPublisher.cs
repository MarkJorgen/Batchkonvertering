using System.Collections.Generic;
using dk.gi.app.contact.selskab.Application.Abstractions;
using dk.gi.app.contact.selskab.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.selskab.Infrastructure.Messaging
{
    public sealed class ContactSelskabJobPublisher : IContactSelskabJobPublisher
    {
        private readonly ContactSelskabSettings _settings;
        private readonly ContactSelskabServiceBusSender _sender;
        private readonly IJobLogger _logger;

        public ContactSelskabJobPublisher(ContactSelskabSettings settings, ContactSelskabServiceBusSender sender, IJobLogger logger)
        {
            _settings = settings;
            _sender = sender;
            _logger = logger;
        }

        public ContactSelskabPublishResult Publish(IReadOnlyCollection<ContactSelskabCandidate> candidates, ResolvedServiceBusSettings resolvedServiceBusSettings)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return new ContactSelskabPublishResult(true, 0, "Ingen selskaber kvalificerede til KDK-opdatering.");
            }

            int published = 0;
            int failures = 0;
            int delaySeconds = 0;

            foreach (var candidate in candidates)
            {
                if (_sender.Send(candidate, resolvedServiceBusSettings, delaySeconds))
                {
                    published++;
                }
                else
                {
                    failures++;
                }

                delaySeconds += _settings.QueueScheduleStepSeconds;
            }

            if (failures > 0)
            {
                return new ContactSelskabPublishResult(false, published, "Service Bus-publicering fejlede for " + failures + " selskab(er).");
            }

            _logger.Info("Publicerede " + published + " contact.selskab job(s).");
            return new ContactSelskabPublishResult(true, published, "Publicerede " + published + " KDK-opdateringsjob(s).");
        }
    }
}
