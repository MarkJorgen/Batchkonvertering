using dk.gi.app.konto.startarealtjek.Application.Abstractions;
using dk.gi.app.konto.startarealtjek.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.startarealtjek.Infrastructure.Crm
{
    public sealed class KontoStartArealTjekCrmGateway : IKontoStartArealTjekGateway
    {
        private readonly KontoStartArealTjekSettings _settings;
        private readonly IKontoStartArealTjekWorkflow _workflow;
        private readonly IJobLogger _logger;

        public KontoStartArealTjekCrmGateway(KontoStartArealTjekSettings settings, IKontoStartArealTjekWorkflow workflow, IJobLogger logger)
        {
            _settings = settings;
            _workflow = workflow;
            _logger = logger;
        }

        public KontoStartArealTjekExecutionSummary Execute(KontoStartArealTjekRequest request)
        {
            if (_settings.DryRun)
            {
                _logger.Info("DRYRUN-mode aktiv. Kører lokal Dataverse-scan og vurdering uden CRM-opdatering og uden Service Bus-publicering.");
            }
            else
            {
                _logger.Info("RUN-mode aktiv. Kører lokal Dataverse-scan, opdaterer ap_emneforarealtjek og publicerer arealtjek-job til Service Bus.");
            }

            return _workflow.Execute(request);
        }
    }
}
