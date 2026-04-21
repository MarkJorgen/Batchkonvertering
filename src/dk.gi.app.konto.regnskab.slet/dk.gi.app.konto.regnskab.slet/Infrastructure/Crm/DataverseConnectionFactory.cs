using dk.gi.app.konto.regnskab.slet.Application.Models;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace dk.gi.app.konto.regnskab.slet.Infrastructure.Crm
{
    public sealed class DataverseConnectionFactory
    {
        private readonly RegnskabSletSettings _settings;

        public DataverseConnectionFactory(RegnskabSletSettings settings)
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
