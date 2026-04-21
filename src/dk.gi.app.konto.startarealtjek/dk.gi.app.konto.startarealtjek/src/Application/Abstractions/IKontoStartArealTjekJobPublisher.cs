using System.Collections.Generic;
using dk.gi.app.konto.startarealtjek.Application.Models;

namespace dk.gi.app.konto.startarealtjek.Application.Abstractions
{
    public interface IKontoStartArealTjekJobPublisher
    {
        KontoStartArealTjekPublishResult Publish(IReadOnlyCollection<KontoStartArealTjekCandidate> candidates, ResolvedServiceBusSettings resolvedServiceBusSettings);
    }
}
