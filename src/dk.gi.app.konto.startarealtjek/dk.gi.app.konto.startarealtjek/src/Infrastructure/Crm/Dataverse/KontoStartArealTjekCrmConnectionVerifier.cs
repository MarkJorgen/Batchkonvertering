using System;
using dk.gi.app.konto.startarealtjek.Application.Abstractions;
using dk.gi.app.konto.startarealtjek.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.startarealtjek.Infrastructure.Crm
{
    public sealed class KontoStartArealTjekCrmConnectionVerifier : IKontoStartArealTjekCrmConnectionVerifier
    {
        private readonly IKontoStartArealTjekScanClientFactory _clientFactory;
        private readonly IJobLogger _logger;

        public KontoStartArealTjekCrmConnectionVerifier(IKontoStartArealTjekScanClientFactory clientFactory, IJobLogger logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public KontoStartArealTjekExecutionSummary Verify()
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
                return new KontoStartArealTjekExecutionSummary(false, 0, 0, 0, 0, "VERIFYCRM fejlede: " + ex.Message, ex.GetType().FullName);
            }
        }
    }
}
