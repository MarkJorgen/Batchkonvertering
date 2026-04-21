using System;
using dk.gi.app.contact.registrering.optaelling.Application.Abstractions;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.registrering.optaelling.Infrastructure.Crm
{
    public sealed class ContactRegistreringOptaellingCrmGateway : IContactRegistreringOptaellingGateway
    {
        private readonly ContactRegistreringOptaellingSettings _settings;
        private readonly IContactRegistreringWorkflow _workflow;
        private readonly IJobLogger _logger;

        public ContactRegistreringOptaellingCrmGateway(
            ContactRegistreringOptaellingSettings settings,
            IContactRegistreringWorkflow workflow,
            IJobLogger logger)
        {
            _settings = settings;
            _workflow = workflow;
            _logger = logger;
        }

        public ContactRegistreringExecutionSummary Execute(ContactRegistreringOptaellingRequest request)
        {
            string registreringScope = request.RegistreringId.HasValue
                ? request.RegistreringId.Value.ToString()
                : "alle registreringer";

            if (_settings.DryRun)
            {
                return new ContactRegistreringExecutionSummary(
                    success: true,
                    closedExpiredTreklipOwnerRegistrations: true,
                    createdJobsForContacts: true,
                    message: "DRYRUN: workflowet er mappet til de to konkrete ContactBLL-trin, men der udføres ingen faktiske CRM-kald endnu. Scope=" + registreringScope,
                    source: "DRYRUN mapper");
            }

            _logger.Info("RUN-mode aktiv. Kalder source-baseret ContactBLL-erstatning via lokal Dataverse-adapter.");
            return _workflow.Execute(request.RegistreringId);
        }
    }
}
