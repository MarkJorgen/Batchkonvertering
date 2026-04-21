using System.Collections.Generic;
using dk.gi.app.contact.selskab.Application.Models;

namespace dk.gi.app.contact.selskab.Application.Abstractions
{
    public interface IContactSelskabJobPublisher
    {
        ContactSelskabPublishResult Publish(IReadOnlyCollection<ContactSelskabCandidate> candidates, ResolvedServiceBusSettings resolvedServiceBusSettings);
    }
}
