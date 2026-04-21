using dk.gi.app.konto.satser.opret.Application.Models;
using dk.gi.crm;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace dk.gi.app.konto.satser.opret.Infrastructure.Crm
{
    public sealed class DataverseConnectionFactory
    {
        private readonly OpretSatserSettings _settings;

        public DataverseConnectionFactory(OpretSatserSettings settings)
        {
            _settings = settings;
        }

        public ServiceClient CreateClient()
        {
            var connectionString = Gi.Batch.Shared.Crm.CrmConnectionStringFactory.Create(
                _settings.CrmConnectionTemplate,
                _settings.CrmServerName,
                _settings.CrmClientId,
                _settings.CrmClientSecret,
                _settings.CrmAuthority,
                _settings.AuthorityMode);

            return new ServiceClient(connectionString);
        }

        public CrmContext CreateLegacyContext()
        {
            var connectionString = Gi.Batch.Shared.Crm.CrmConnectionStringFactory.Create(
                _settings.CrmConnectionTemplate,
                _settings.CrmServerName,
                _settings.CrmClientId,
                _settings.CrmClientSecret,
                _settings.CrmAuthority,
                _settings.AuthorityMode);

            return new CrmContext(connectionString);
        }
    }
}
