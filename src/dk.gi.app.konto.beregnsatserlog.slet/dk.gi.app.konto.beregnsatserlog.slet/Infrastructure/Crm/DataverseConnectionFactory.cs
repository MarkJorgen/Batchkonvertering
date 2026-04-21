using dk.gi.app.konto.beregnsatserlog.slet.Application.Models;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace dk.gi.app.konto.beregnsatserlog.slet.Infrastructure.Crm
{
    public sealed class DataverseConnectionFactory
    {
        private readonly SletBeregnSatserLogSettings _settings;

        public DataverseConnectionFactory(SletBeregnSatserLogSettings settings)
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
