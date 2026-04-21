using System;
using dk.gi.app.contact.selskab.Application.Abstractions;
using dk.gi.app.contact.selskab.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.selskab.Infrastructure.Crm
{
    public sealed class ContactSelskabCrmConnectionVerifier : IContactSelskabCrmConnectionVerifier
    {
        private readonly IContactSelskabScanClientFactory _clientFactory;
        private readonly IJobLogger _logger;

        public ContactSelskabCrmConnectionVerifier(IContactSelskabScanClientFactory clientFactory, IJobLogger logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public ContactSelskabExecutionSummary Verify()
        {
            try
            {
                _logger.Info("VERIFYCRM-mode aktiv. Validerer CRM-forbindelse uden at køre workflow.");
                using (var client = _clientFactory.Create())
                {
                    var summary = client.VerifyConnection();
                    _logger.Info("Dataverse-forbindelse valideret i VERIFYCRM-mode.");
                    return summary;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("VERIFYCRM fejlede. " + ex.Message);
                return new ContactSelskabExecutionSummary(false, 0, 0, 0, "VERIFYCRM fejlede: " + ex.Message, ex.GetType().FullName);
            }
        }
    }
}
