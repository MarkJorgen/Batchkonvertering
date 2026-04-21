using System;
using System.Collections.Generic;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Abstractions;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Messaging
{
    public sealed class RunoutRegistreringJobPublisher : IRunoutRegistreringJobPublisher
    {
        private readonly ContactRegistreringUdloebneOptaellingSettings _settings;
        private readonly RunoutRegistreringServiceBusSender _sender;
        private readonly IJobLogger _logger;

        public RunoutRegistreringJobPublisher(ContactRegistreringUdloebneOptaellingSettings settings, RunoutRegistreringServiceBusSender sender, IJobLogger logger)
        {
            _settings = settings;
            _sender = sender;
            _logger = logger;
        }

        public RunoutRegistreringPublishResult Publish(IReadOnlyCollection<RunoutRegistreringCandidate> candidates, IReadOnlyCollection<Guid> closedTreklipIds, ResolvedServiceBusSettings resolvedServiceBusSettings)
        {
            if (candidates == null || candidates.Count == 0)
                return new RunoutRegistreringPublishResult(true, 0, 0, "Ingen kandidater fundet.");

            if (closedTreklipIds == null || closedTreklipIds.Count == 0)
                return new RunoutRegistreringPublishResult(true, 0, candidates.Count, "Ingen afsluttede treklip fundet. Lokal implementation bevarer derfor legacy-adfærden og publicerer ingen jobs.");

            int published = 0;
            int skipped = 0;
            int failures = 0;
            var closedLookup = new HashSet<Guid>(closedTreklipIds);

            foreach (var candidate in candidates)
            {
                if (candidate.TreklipId.HasValue && !closedLookup.Contains(candidate.TreklipId.Value))
                {
                    skipped++;
                    _logger.Info("Registrering springes over, fordi treklip ikke er afsluttet. RegistreringId=" + candidate.Id);
                    continue;
                }

                if (_sender.Send(candidate, resolvedServiceBusSettings)) published++;
                else failures++;
            }

            if (failures > 0)
                return new RunoutRegistreringPublishResult(false, published, skipped, "Publicering til Service Bus fejlede for " + failures + " registrering(er).");

            return new RunoutRegistreringPublishResult(true, published, skipped, "Publicerede " + published + " runout-job(s) og sprang " + skipped + " over.");
        }
    }
}
