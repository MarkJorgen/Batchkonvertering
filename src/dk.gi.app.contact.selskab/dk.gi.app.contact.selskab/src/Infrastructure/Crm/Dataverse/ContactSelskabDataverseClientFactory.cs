using dk.gi.app.contact.selskab.Application.Abstractions;
using dk.gi.app.contact.selskab.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.selskab.Infrastructure.Crm
{
    public sealed class ContactSelskabDataverseClientFactory : IContactSelskabScanClientFactory
    {
        private readonly ContactSelskabSettings _settings;
        private readonly IJobLogger _logger;

        public ContactSelskabDataverseClientFactory(ContactSelskabSettings settings, IJobLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public IContactSelskabScanClient Create()
        {
            _logger.Info("Opretter Dataverse klient. Sanitized connection string: " + CrmConnectionStringFactory.CreateSanitized(_settings));
            var client = new ContactSelskabDataverseClient(
                CrmConnectionStringFactory.Create(_settings),
                _settings.TimeOutMinutes,
                message => _logger.Info(message));
            _logger.Info("Dataverse klient oprettet.");
            return client;
        }
    }
}
