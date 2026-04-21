using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.satser.opret.Application.Contracts;
using dk.gi.app.konto.satser.opret.Application.Models;
using Microsoft.Extensions.Logging;

namespace dk.gi.app.konto.satser.opret.Application.Services
{
    public sealed class OpretSatserOrchestrator
    {
        private readonly IOpretSatserRepository _repository;
        private readonly IConnectivityVerifier _connectivityVerifier;
        private readonly ILogger _logger;

        public OpretSatserOrchestrator(
            IOpretSatserRepository repository,
            IConnectivityVerifier connectivityVerifier,
            ILogger logger)
        {
            _repository = repository;
            _connectivityVerifier = connectivityVerifier;
            _logger = logger;
        }

        public async Task<ExecutionReport> ExecuteAsync(OpretSatserSettings settings, CancellationToken cancellationToken = default)
        {
            var report = new ExecutionReport();

            _logger.LogInformation(
                "Starter oprettelse af satser. Mode={Mode}, SatsAar={SatsAar}",
                settings.Mode,
                settings.SatsAar);

            await _connectivityVerifier.VerifyAsync(cancellationToken).ConfigureAwait(false);
            report.ConnectivityVerified = true;

            if (settings.Mode == JobExecutionMode.VerifyCrm)
            {
                _logger.LogInformation("VERIFYCRM gennemført uden yderligere behandling.");
                return report;
            }

            report.CandidateCount = await _repository.GetCandidateCountAsync(settings.SatsAar, cancellationToken).ConfigureAwait(false);

            if (settings.Mode == JobExecutionMode.DryRun)
            {
                _logger.LogInformation(
                    "DRYRUN afsluttet. {CandidateCount} kontoer er kandidater til satsoprettelse for år {SatsAar}.",
                    report.CandidateCount,
                    settings.SatsAar);
                return report;
            }

            report.CreatedCount = await _repository.CreateAsync(settings.SatsAar, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "RUN afsluttet. {CreatedCount} satser blev oprettet for år {SatsAar}.",
                report.CreatedCount,
                settings.SatsAar);

            return report;
        }
    }
}
