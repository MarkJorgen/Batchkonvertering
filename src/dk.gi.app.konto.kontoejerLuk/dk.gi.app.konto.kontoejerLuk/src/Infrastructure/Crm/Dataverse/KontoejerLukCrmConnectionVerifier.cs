using System;
using dk.gi.app.konto.kontoejerLuk.Application.Abstractions;
using dk.gi.app.konto.kontoejerLuk.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.kontoejerLuk.Infrastructure.Crm.Dataverse
{
    public sealed class KontoejerLukCrmConnectionVerifier : IKontoejerLukCrmConnectionVerifier
    {
        private readonly IKontoejerLukScanClientFactory _clientFactory;
        private readonly IJobLogger _logger;

        public KontoejerLukCrmConnectionVerifier(IKontoejerLukScanClientFactory clientFactory, IJobLogger logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public KontoejerLukExecutionSummary Verify()
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
                return new KontoejerLukExecutionSummary(false, 0, 0, 0, "VERIFYCRM fejlede: " + ex.Message, ex.GetType().FullName);
            }
        }
    }
}
