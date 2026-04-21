using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.satser.slet.Application.Contracts;
using dk.gi.app.konto.satser.slet.Application.Models;
using Microsoft.Extensions.Logging;

namespace dk.gi.app.konto.satser.slet.Application.Services
{
    public sealed class SletSatserOrchestrator
    {
        private readonly ISatserRepository _repository;
        private readonly IConnectivityVerifier _connectivityVerifier;
        private readonly ILogger _logger;

        public SletSatserOrchestrator(
            ISatserRepository repository,
            IConnectivityVerifier connectivityVerifier,
            ILogger logger)
        {
            _repository = repository;
            _connectivityVerifier = connectivityVerifier;
            _logger = logger;
        }

        public async Task<ExecutionReport> ExecuteAsync(SletSatserSettings settings, CancellationToken cancellationToken = default)
        {
            var report = new ExecutionReport();

            _logger.LogInformation(
                "Starter sletning af satser. Mode={Mode}, SatsAar={SatsAar}",
                settings.Mode,
                settings.SatsAar);

            await _connectivityVerifier.VerifyAsync(cancellationToken).ConfigureAwait(false);
            report.ConnectivityVerified = true;

            if (settings.Mode == JobExecutionMode.VerifyCrm)
            {
                _logger.LogInformation("VERIFYCRM gennemført uden yderligere behandling.");
                return report;
            }

            var candidates = await _repository.GetCandidatesAsync(settings.SatsAar, cancellationToken).ConfigureAwait(false);
            report.CandidateCount = candidates.Count;

            if (settings.Mode == JobExecutionMode.DryRun)
            {
                _logger.LogInformation(
                    "DRYRUN afsluttet. {CandidateCount} satser-records er kandidater til sletning for år {SatsAar}.",
                    report.CandidateCount,
                    settings.SatsAar);
                return report;
            }

            foreach (var candidate in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _repository.DeleteAsync(candidate, cancellationToken).ConfigureAwait(false);
                report.DeletedCount += 1;
            }

            _logger.LogInformation(
                "RUN afsluttet. {DeletedCount} satser-records blev slettet for år {SatsAar}.",
                report.DeletedCount,
                settings.SatsAar);

            return report;
        }
    }
}
