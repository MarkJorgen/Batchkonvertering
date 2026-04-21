using dk.gi.app.konto.kontoejerLuk.Application.Abstractions;
using dk.gi.app.konto.kontoejerLuk.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.kontoejerLuk.Infrastructure.Crm
{
    public sealed class KontoejerLukCrmGateway : IKontoejerLukGateway
    {
        private readonly KontoejerLukSettings _settings;
        private readonly IKontoejerLukWorkflow _workflow;
        private readonly IJobLogger _logger;

        public KontoejerLukCrmGateway(KontoejerLukSettings settings, IKontoejerLukWorkflow workflow, IJobLogger logger)
        {
            _settings = settings;
            _workflow = workflow;
            _logger = logger;
        }

        public KontoejerLukExecutionSummary Execute(KontoejerLukRequest request)
        {
            if (_settings.DryRun)
                _logger.Info("DRYRUN-mode aktiv. Kører lokal Dataverse-scan og planlægning uden CRM-opdatering.");
            else
                _logger.Info("RUN-mode aktiv. Kører lokal Dataverse-scan og opdaterer ap_slutdato for åbne kontoejere.");

            return _workflow.Execute(request);
        }
    }
}
