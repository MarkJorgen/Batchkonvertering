using dk.gi.app.contact.selskab.Application.Abstractions;
using dk.gi.app.contact.selskab.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.selskab.Infrastructure.Crm
{
    public sealed class ContactSelskabCrmGateway : IContactSelskabGateway
    {
        private readonly ContactSelskabSettings _settings;
        private readonly IContactSelskabWorkflow _workflow;
        private readonly IJobLogger _logger;

        public ContactSelskabCrmGateway(ContactSelskabSettings settings, IContactSelskabWorkflow workflow, IJobLogger logger)
        {
            _settings = settings;
            _workflow = workflow;
            _logger = logger;
        }

        public ContactSelskabExecutionSummary Execute(ContactSelskabRequest request)
        {
            if (_settings.DryRun)
            {
                _logger.Info("DRYRUN-mode aktiv. Kører lokal Dataverse-scan og kvalificering uden Service Bus-publicering.");
            }
            else
            {
                _logger.Info("RUN-mode aktiv. Kører lokal Dataverse-scan med Service Bus-publicering af KDK-opdateringer.");
            }

            return _workflow.Execute(request);
        }
    }
}
