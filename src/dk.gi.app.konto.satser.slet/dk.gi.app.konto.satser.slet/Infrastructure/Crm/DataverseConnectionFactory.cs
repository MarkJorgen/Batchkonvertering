using dk.gi.app.konto.satser.slet.Application.Models;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace dk.gi.app.konto.satser.slet.Infrastructure.Crm
{
    public sealed class DataverseConnectionFactory
    {
        private readonly SletSatserSettings _settings;

        public DataverseConnectionFactory(SletSatserSettings settings)
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
    }
}
