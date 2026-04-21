using dk.gi.app.konto.afslutarealsager.Application.Abstractions;
using dk.gi.app.konto.afslutarealsager.Application.Models;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Messaging
{
    public sealed class KontoAfslutArealSagerCloseoutPublisher : IKontoAfslutArealSagerCloseoutPublisher
    {
        private readonly KontoAfslutArealSagerCloseoutServiceBusSender _sender;

        public KontoAfslutArealSagerCloseoutPublisher(KontoAfslutArealSagerCloseoutServiceBusSender sender)
        {
            _sender = sender;
        }

        public bool Publish(KontoAfslutArealSagerCandidate candidate, ResolvedServiceBusSettings resolvedServiceBusSettings, int scheduleDelaySeconds)
        {
            return _sender.Send(candidate, resolvedServiceBusSettings, scheduleDelaySeconds);
        }
    }
}
