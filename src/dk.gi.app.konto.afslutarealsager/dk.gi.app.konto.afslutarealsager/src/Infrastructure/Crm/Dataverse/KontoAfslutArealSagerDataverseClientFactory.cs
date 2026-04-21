using dk.gi.app.konto.afslutarealsager.Application.Abstractions;
using dk.gi.app.konto.afslutarealsager.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Crm.Dataverse
{
    public sealed class KontoAfslutArealSagerDataverseClientFactory : IKontoAfslutArealSagerScanClientFactory
    {
        private readonly KontoAfslutArealSagerSettings _settings;
        private readonly IJobLogger _logger;

        public KontoAfslutArealSagerDataverseClientFactory(KontoAfslutArealSagerSettings settings, IJobLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public IKontoAfslutArealSagerScanClient Create()
        {
            _logger.Info("Opretter Dataverse klient. Sanitized connection string: " + CrmConnectionStringFactory.CreateSanitized(_settings));
            var client = new KontoAfslutArealSagerDataverseClient(_settings, CrmConnectionStringFactory.Create(_settings), _settings.TimeOutMinutes, _logger.Info);
            _logger.Info("Dataverse klient oprettet.");
            return client;
        }
    }
}
