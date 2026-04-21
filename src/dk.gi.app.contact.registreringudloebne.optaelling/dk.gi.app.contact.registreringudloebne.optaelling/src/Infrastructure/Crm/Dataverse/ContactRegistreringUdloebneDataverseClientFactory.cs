using dk.gi.app.contact.registreringudloebne.optaelling.Application.Abstractions;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Crm
{
    public sealed class ContactRegistreringUdloebneDataverseClientFactory : IRunoutRegistreringScanClientFactory
    {
        private readonly ContactRegistreringUdloebneOptaellingSettings _settings;
        private readonly IJobLogger _logger;

        public ContactRegistreringUdloebneDataverseClientFactory(ContactRegistreringUdloebneOptaellingSettings settings, IJobLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public IRunoutRegistreringScanClient Create()
        {
            _logger.Info("Opretter Dataverse klient. Sanitized connection string: " + CrmConnectionStringFactory.CreateSanitized(_settings));
            return new ContactRegistreringUdloebneDataverseClient(CrmConnectionStringFactory.Create(_settings));
        }
    }
}
