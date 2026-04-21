using System;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Abstractions;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Crm
{
    public sealed class ContactRegistreringUdloebneCrmConnectionVerifier : IContactRegistreringUdloebneCrmConnectionVerifier
    {
        private readonly IRunoutRegistreringScanClientFactory _clientFactory;
        private readonly IJobLogger _logger;

        public ContactRegistreringUdloebneCrmConnectionVerifier(IRunoutRegistreringScanClientFactory clientFactory, IJobLogger logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public ContactRegistreringUdloebneExecutionSummary Verify()
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
                return new ContactRegistreringUdloebneExecutionSummary(false, 0, 0, "VERIFYCRM fejlede: " + ex.Message, ex.GetType().FullName);
            }
        }
    }
}
