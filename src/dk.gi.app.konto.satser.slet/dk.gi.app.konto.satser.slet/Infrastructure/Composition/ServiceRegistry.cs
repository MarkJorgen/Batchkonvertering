using dk.gi.app.konto.satser.slet.Application.Contracts;
using dk.gi.app.konto.satser.slet.Application.Models;
using dk.gi.app.konto.satser.slet.Application.Services;
using dk.gi.app.konto.satser.slet.Infrastructure.Crm;
using Microsoft.Extensions.Logging;

namespace dk.gi.app.konto.satser.slet.Infrastructure.Composition
{
    public sealed class ServiceRegistry
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly SletSatserSettings _settings;

        public ServiceRegistry(ILoggerFactory loggerFactory, SletSatserSettings settings)
        {
            _loggerFactory = loggerFactory;
            _settings = settings;
        }

        public SletSatserOrchestrator CreateOrchestrator()
        {
            var connectionFactory = new DataverseConnectionFactory(_settings);
            ISatserRepository repository = new DataverseSatserRepository(connectionFactory, _loggerFactory.CreateLogger<DataverseSatserRepository>());
            IConnectivityVerifier verifier = new DataverseConnectivityVerifier(connectionFactory, _loggerFactory.CreateLogger<DataverseConnectivityVerifier>());

            return new SletSatserOrchestrator(
                repository,
                verifier,
                _loggerFactory.CreateLogger<SletSatserOrchestrator>());
        }
    }
}
