using dk.gi.app.konto.afslutarealsager.Application.Abstractions;
using dk.gi.app.konto.afslutarealsager.Application.Models;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Messaging
{
    public sealed class KontoAfslutArealSagerArealSumPublisher : IKontoAfslutArealSagerArealSumPublisher
    {
        private readonly KontoAfslutArealSagerArealSumServiceBusSender _sender;

        public KontoAfslutArealSagerArealSumPublisher(KontoAfslutArealSagerArealSumServiceBusSender sender)
        {
            _sender = sender;
        }

        public bool Publish(KontoAfslutArealSagerCandidate candidate, string areaId, ResolvedServiceBusSettings resolvedServiceBusSettings, int scheduleDelaySeconds)
        {
            return _sender.Send(candidate, areaId, resolvedServiceBusSettings, scheduleDelaySeconds);
        }
    }
}
