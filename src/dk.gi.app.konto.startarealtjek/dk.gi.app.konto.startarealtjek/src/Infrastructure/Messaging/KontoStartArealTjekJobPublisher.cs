using System.Collections.Generic;
using dk.gi.app.konto.startarealtjek.Application.Abstractions;
using dk.gi.app.konto.startarealtjek.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.startarealtjek.Infrastructure.Messaging
{
    public sealed class KontoStartArealTjekJobPublisher : IKontoStartArealTjekJobPublisher
    {
        private readonly KontoStartArealTjekSettings _settings;
        private readonly KontoStartArealTjekServiceBusSender _sender;
        private readonly IJobLogger _logger;

        public KontoStartArealTjekJobPublisher(KontoStartArealTjekSettings settings, KontoStartArealTjekServiceBusSender sender, IJobLogger logger)
        {
            _settings = settings;
            _sender = sender;
            _logger = logger;
        }

        public KontoStartArealTjekPublishResult Publish(IReadOnlyCollection<KontoStartArealTjekCandidate> candidates, ResolvedServiceBusSettings resolvedServiceBusSettings)
        {
            if (candidates == null || candidates.Count == 0)
                return new KontoStartArealTjekPublishResult(true, 0, "Ingen konti kvalificerede til arealtjek-job.");

            _logger.Info("Starter Service Bus-publicering for " + candidates.Count + " konto.startarealtjek job(s).");

            int published = 0;
            int failures = 0;
            int delaySeconds = 0;

            foreach (var candidate in candidates)
            {
                if (_sender.Send(candidate, resolvedServiceBusSettings, delaySeconds)) published++;
                else failures++;
                delaySeconds += _settings.QueueScheduleStepSeconds;
            }

            if (failures > 0)
                return new KontoStartArealTjekPublishResult(false, published, "Service Bus-publicering fejlede for " + failures + " konto(er).");

            _logger.Info("Publicerede " + published + " konto.startarealtjek job(s).");
            return new KontoStartArealTjekPublishResult(true, published, "Publicerede " + published + " arealtjek-job(s).");
        }
    }
}
