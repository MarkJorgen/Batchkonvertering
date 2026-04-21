using dk.gi.app.contact.lassox.ophoer.Application.Abstractions;
using dk.gi.app.contact.lassox.ophoer.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.lassox.ophoer.Infrastructure.Crm
{
    public sealed class LassoXOphoerCrmGateway : ILassoXOphoerGateway
    {
        private readonly LassoXOphoerSettings _settings;
        private readonly ILassoXOphoerWorkflow _workflow;
        private readonly IJobLogger _logger;

        public LassoXOphoerCrmGateway(LassoXOphoerSettings settings, ILassoXOphoerWorkflow workflow, IJobLogger logger)
        {
            _settings = settings;
            _workflow = workflow;
            _logger = logger;
        }

        public LassoXOphoerExecutionSummary Execute(LassoXOphoerRequest request)
        {
            if (_settings.DryRun)
            {
                _logger.Info("DRYRUN-mode aktiv. Kører lokal Dataverse-scan i simulation uden writes.");
            }
            else
            {
                _logger.Info("RUN-mode aktiv. Kører lokal Dataverse-scan med eksplicit write-mode.");
            }

            return _workflow.Execute(request);
        }
    }
}
