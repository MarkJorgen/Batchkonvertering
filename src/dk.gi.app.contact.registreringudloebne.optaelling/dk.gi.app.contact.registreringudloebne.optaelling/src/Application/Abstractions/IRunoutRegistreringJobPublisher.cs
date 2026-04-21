using System;
using System.Collections.Generic;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Application.Abstractions
{
    public interface IRunoutRegistreringJobPublisher
    {
        RunoutRegistreringPublishResult Publish(
            IReadOnlyCollection<RunoutRegistreringCandidate> candidates,
            IReadOnlyCollection<Guid> closedTreklipIds,
            ResolvedServiceBusSettings resolvedServiceBusSettings);
    }
}
