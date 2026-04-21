using System;
using dk.gi.app.contact.registrering.optaelling.Application.Abstractions;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.registrering.optaelling.Infrastructure.Crm
{
    public sealed class ContactRegistreringDataverseClientFactory : IContactRegistreringWorkflowClientFactory
    {
        private readonly ContactRegistreringOptaellingSettings _settings;
        private readonly IJobLogger _logger;

        public ContactRegistreringDataverseClientFactory(
            ContactRegistreringOptaellingSettings settings,
            IJobLogger logger)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger;
        }

        public IContactRegistreringWorkflowClient Create()
        {
            string connectionString = CrmConnectionStringFactory.Create(_settings);
            string sanitized = CrmConnectionStringFactory.CreateSanitized(_settings);

            _logger?.Info("Dataverse connect diagnostics: CrmServerName='" + (_settings.CrmServerName ?? string.Empty) + "', CrmAuthority='" + (_settings.CrmAuthority ?? string.Empty) + "', CrmClientId configured=" + (!string.IsNullOrWhiteSpace(_settings.CrmClientId) ? "Ja" : "Nej") + ", CrmClientSecret configured=" + (!string.IsNullOrWhiteSpace(_settings.CrmClientSecret) ? "Ja" : "Nej") + ", CrmClientSecret looks like guid=" + (Infrastructure.Config.CrmScalarSettingNormalizer.LooksLikeGuid(_settings.CrmClientSecret) ? "Ja" : "Nej") + ", CrmClientSecret length=" + ((_settings.CrmClientSecret ?? string.Empty).Length));
            _logger?.Info("Dataverse sanitized connection string: " + sanitized);

            try
            {
                return new ContactRegistreringDataverseClient(connectionString);
            }
            catch (Exception ex)
            {
                _logger?.Error("Dataverse client creation failed. Sanitized connection string: " + sanitized, ex);
                throw;
            }
        }
    }
}
