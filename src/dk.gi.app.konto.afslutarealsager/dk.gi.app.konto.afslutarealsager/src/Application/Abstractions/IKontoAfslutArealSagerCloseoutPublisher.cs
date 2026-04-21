using dk.gi.app.konto.afslutarealsager.Application.Models;

namespace dk.gi.app.konto.afslutarealsager.Application.Abstractions
{
    public interface IKontoAfslutArealSagerCloseoutPublisher
    {
        bool Publish(KontoAfslutArealSagerCandidate candidate, ResolvedServiceBusSettings resolvedServiceBusSettings, int scheduleDelaySeconds);
    }
}
