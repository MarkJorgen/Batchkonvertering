using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.regnskab.slet.Application.Contracts;
using dk.gi.app.konto.regnskab.slet.Application.Models;
using Microsoft.Extensions.Logging;

namespace dk.gi.app.konto.regnskab.slet.Infrastructure.Messaging
{
    public sealed class RegnskabSletPublisher : IRegnskabSletPublisher
    {
        private readonly RegnskabSletSettings _settings;
        private readonly RegnskabSletServiceBusSender _sender;
        private readonly ILogger _logger;

        public RegnskabSletPublisher(RegnskabSletSettings settings, RegnskabSletServiceBusSender sender, ILogger logger)
        {
            _settings = settings;
            _sender = sender;
            _logger = logger;
        }

        public Task<int> PublishAsync(IReadOnlyCollection<KontoCandidate> candidates, ResolvedServiceBusSettings resolvedServiceBusSettings, CancellationToken cancellationToken)
        {
            int published = 0;
            int delaySeconds = 0;
            foreach (var candidate in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (_sender.Send(candidate, resolvedServiceBusSettings, delaySeconds)) published++;
                delaySeconds += _settings.DelayStepSeconds;
            }
            _logger.LogInformation("Publicering afsluttet. Requested={Requested}, Published={Published}", candidates.Count, published);
            return Task.FromResult(published);
        }
    }
}
