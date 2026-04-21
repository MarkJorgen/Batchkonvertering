using dk.gi.app.konto.afslutarealsager.Application.Abstractions;
using dk.gi.app.konto.afslutarealsager.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Crm.Dataverse
{
    public sealed class KontoAfslutArealSagerCrmConnectionVerifier : IKontoAfslutArealSagerCrmConnectionVerifier
    {
        private readonly IKontoAfslutArealSagerScanClientFactory _scanClientFactory;
        private readonly IJobLogger _logger;

        public KontoAfslutArealSagerCrmConnectionVerifier(IKontoAfslutArealSagerScanClientFactory scanClientFactory, IJobLogger logger)
        {
            _scanClientFactory = scanClientFactory;
            _logger = logger;
        }

        public KontoAfslutArealSagerExecutionSummary Verify()
        {
            using (var client = _scanClientFactory.Create())
            {
                client.EnsureConnection();
                _logger.Info("Dataverse-forbindelse valideret i VERIFYCRM-mode.");
                return KontoAfslutArealSagerExecutionSummary.Ok("Dataverse", "Forbindelse etableret og metadata læsbar.");
            }
        }
    }
}
