using dk.gi.app.konto.afslutarealsager.Application.Models;

namespace dk.gi.app.konto.afslutarealsager.Application.Abstractions
{
    public interface IKontoAfslutArealSagerArealSumPublisher
    {
        bool Publish(KontoAfslutArealSagerCandidate candidate, string areaId, ResolvedServiceBusSettings resolvedServiceBusSettings, int scheduleDelaySeconds);
    }
}
