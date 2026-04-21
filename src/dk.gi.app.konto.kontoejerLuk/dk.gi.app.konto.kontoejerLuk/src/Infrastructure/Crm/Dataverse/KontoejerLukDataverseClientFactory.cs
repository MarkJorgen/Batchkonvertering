using dk.gi.app.konto.kontoejerLuk.Application.Abstractions;
using dk.gi.app.konto.kontoejerLuk.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.kontoejerLuk.Infrastructure.Crm.Dataverse
{
    public sealed class KontoejerLukDataverseClientFactory : IKontoejerLukScanClientFactory
    {
        private readonly KontoejerLukSettings _settings;
        private readonly IJobLogger _logger;

        public KontoejerLukDataverseClientFactory(KontoejerLukSettings settings, IJobLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public IKontoejerLukScanClient Create()
        {
            _logger.Info("Opretter Dataverse klient. Sanitized connection string: " + CrmConnectionStringFactory.CreateSanitized(_settings));
            var client = new KontoejerLukDataverseClient(
                CrmConnectionStringFactory.Create(_settings),
                _settings.TimeOutMinutes,
                message => _logger.Info(message));
            _logger.Info("Dataverse klient oprettet.");
            return client;
        }
    }
}
