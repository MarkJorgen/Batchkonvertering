using dk.gi.app.contact.registreringudloebne.optaelling.Application.Abstractions;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Crm
{
    public sealed class ContactRegistreringUdloebneOptaellingCrmGateway : IContactRegistreringUdloebneOptaellingGateway
    {
        private readonly ContactRegistreringUdloebneOptaellingSettings _settings;
        private readonly IContactRegistreringUdloebneWorkflow _workflow;
        private readonly IJobLogger _logger;

        public ContactRegistreringUdloebneOptaellingCrmGateway(ContactRegistreringUdloebneOptaellingSettings settings, IContactRegistreringUdloebneWorkflow workflow, IJobLogger logger)
        {
            _settings = settings;
            _workflow = workflow;
            _logger = logger;
        }

        public ContactRegistreringUdloebneExecutionSummary Execute(ContactRegistreringUdloebneOptaellingRequest request)
        {
            string registreringScope = request.RegistreringId.HasValue ? request.RegistreringId.Value.ToString() : "alle registreringer";

            if (_settings.DryRun)
            {
                return new ContactRegistreringUdloebneExecutionSummary(
                    true,
                    0,
                    0,
                    "DRYRUN: workflowet er mappet til FindAndCreateJobsForContactWithRonoutRegistrering og publicering til crmpluginjobs/laan, men der udføres ingen faktiske CRM- eller Service Bus-kald endnu. Scope=" + registreringScope,
                    "DRYRUN mapper");
            }

            _logger.Info("RUN-mode aktiv. Kalder lokal Dataverse + Service Bus-erstatning for runout-registreringer.");
            return _workflow.Execute(request.RegistreringId);
        }
    }
}
