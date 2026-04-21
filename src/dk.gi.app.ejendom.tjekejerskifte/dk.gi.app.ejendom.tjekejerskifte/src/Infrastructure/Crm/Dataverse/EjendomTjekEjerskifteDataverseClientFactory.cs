using dk.gi.app.ejendom.tjekejerskifte.Application.Abstractions;
using dk.gi.app.ejendom.tjekejerskifte.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.ejendom.tjekejerskifte.Infrastructure.Crm.Dataverse
{
    public sealed class EjendomTjekEjerskifteDataverseClientFactory : IEjendomTjekEjerskifteScanClientFactory
    {
        private readonly EjendomTjekEjerskifteSettings _settings;
        private readonly IJobLogger _logger;

        public EjendomTjekEjerskifteDataverseClientFactory(EjendomTjekEjerskifteSettings settings, IJobLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public IEjendomTjekEjerskifteScanClient Create()
        {
            _logger.Info("Opretter Dataverse klient. Sanitized connection string: " + CrmConnectionStringFactory.CreateSanitized(_settings));
            var client = new EjendomTjekEjerskifteDataverseClient(
                CrmConnectionStringFactory.Create(_settings),
                _settings.TimeOutMinutes,
                message => _logger.Info(message));
            _logger.Info("Dataverse klient oprettet.");
            return client;
        }
    }
}
