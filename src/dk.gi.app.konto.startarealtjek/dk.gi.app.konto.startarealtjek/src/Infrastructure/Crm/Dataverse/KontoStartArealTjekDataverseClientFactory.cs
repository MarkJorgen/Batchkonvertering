using dk.gi.app.konto.startarealtjek.Application.Abstractions;
using dk.gi.app.konto.startarealtjek.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.startarealtjek.Infrastructure.Crm
{
    public sealed class KontoStartArealTjekDataverseClientFactory : IKontoStartArealTjekScanClientFactory
    {
        private readonly KontoStartArealTjekSettings _settings;
        private readonly IJobLogger _logger;

        public KontoStartArealTjekDataverseClientFactory(KontoStartArealTjekSettings settings, IJobLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public IKontoStartArealTjekScanClient Create()
        {
            _logger.Info("Opretter Dataverse klient. Sanitized connection string: " + CrmConnectionStringFactory.CreateSanitized(_settings));
            var client = new KontoStartArealTjekDataverseClient(
                _settings,
                CrmConnectionStringFactory.Create(_settings),
                _settings.TimeOutMinutes,
                message => _logger.Info(message));
            _logger.Info("Dataverse klient oprettet.");
            return client;
        }
    }
}
