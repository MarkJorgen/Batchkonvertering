using System;
using dk.gi.app.contact.registrering.optaelling.Application.Abstractions;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.registrering.optaelling.Infrastructure.Crm
{
    public sealed class ContactRegistreringCrmConnectionVerifier : IContactRegistreringCrmConnectionVerifier
    {
        private readonly IContactRegistreringWorkflowClientFactory _clientFactory;
        private readonly IJobLogger _logger;

        public ContactRegistreringCrmConnectionVerifier(
            IContactRegistreringWorkflowClientFactory clientFactory,
            IJobLogger logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public ContactRegistreringExecutionSummary Verify()
        {
            try
            {
                _logger.Info("VERIFYCRM-mode aktiv. Validerer CRM-forbindelse uden at køre workflow.");

                using (var client = _clientFactory.Create())
                {
                    ContactRegistreringExecutionSummary summary = client.VerifyConnection();
                    _logger.Info("Dataverse-forbindelse valideret i VERIFYCRM-mode.");
                    return summary;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("VERIFYCRM fejlede. " + ex.Message);
                return new ContactRegistreringExecutionSummary(
                    success: false,
                    closedExpiredTreklipOwnerRegistrations: false,
                    createdJobsForContacts: false,
                    message: "VERIFYCRM fejlede: " + ex.Message,
                    source: ex.GetType().FullName);
            }
        }
    }
}
