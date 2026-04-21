using System;
using dk.gi.app.ejendom.tjekejerskifte.Application.Abstractions;
using dk.gi.app.ejendom.tjekejerskifte.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.ejendom.tjekejerskifte.Infrastructure.Crm.Dataverse
{
    public sealed class EjendomTjekEjerskifteCrmConnectionVerifier : IEjendomTjekEjerskifteCrmConnectionVerifier
    {
        private readonly IEjendomTjekEjerskifteScanClientFactory _clientFactory;
        private readonly IJobLogger _logger;

        public EjendomTjekEjerskifteCrmConnectionVerifier(IEjendomTjekEjerskifteScanClientFactory clientFactory, IJobLogger logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public EjendomTjekEjerskifteExecutionSummary Verify()
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
                return new EjendomTjekEjerskifteExecutionSummary(false, 0, 0, 0, "VERIFYCRM fejlede: " + ex.Message, ex.GetType().FullName);
            }
        }
    }
}
