using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.regnskab.slet.Application.Contracts;
using dk.gi.app.konto.regnskab.slet.Application.Models;
using Microsoft.Extensions.Logging;

namespace dk.gi.app.konto.regnskab.slet.Application.Services
{
    public sealed class RegnskabSletOrchestrator
    {
        private readonly IRegnskabSletRepository _repository;
        private readonly IRegnskabSletPublisher _publisher;
        private readonly IConnectivityVerifier _connectivityVerifier;
        private readonly ILogger _logger;

        public RegnskabSletOrchestrator(IRegnskabSletRepository repository, IRegnskabSletPublisher publisher, IConnectivityVerifier connectivityVerifier, ILogger logger)
        {
            _repository = repository;
            _publisher = publisher;
            _connectivityVerifier = connectivityVerifier;
            _logger = logger;
        }

        public async Task<ExecutionReport> ExecuteAsync(RegnskabSletSettings settings, CancellationToken cancellationToken = default)
        {
            var report = new ExecutionReport();

            _logger.LogInformation("Starter regnskab.slet. Mode={Mode}", settings.Mode);
            await _connectivityVerifier.VerifyAsync(cancellationToken).ConfigureAwait(false);
            report.ConnectivityVerified = true;

            if (settings.Mode == JobExecutionMode.VerifyCrm)
            {
                _logger.LogInformation("VERIFYCRM gennemført uden yderligere behandling.");
                return report;
            }

            var candidates = await _repository.GetCandidatesAsync(cancellationToken).ConfigureAwait(false);
            report.SelectedAccountCount = candidates.Count;

            if (settings.Mode == JobExecutionMode.DryRun)
            {
                _logger.LogInformation("DRYRUN afsluttet. {Count} konti er udvalgt til kø-publicering.", report.SelectedAccountCount);
                return report;
            }

            var resolved = await _repository.ResolveServiceBusSettingsAsync(cancellationToken).ConfigureAwait(false);
            report.PublishedCount = await _publisher.PublishAsync(candidates, resolved, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("RUN afsluttet. {Count} kø-job blev publiceret.", report.PublishedCount);
            return report;
        }
    }
}
