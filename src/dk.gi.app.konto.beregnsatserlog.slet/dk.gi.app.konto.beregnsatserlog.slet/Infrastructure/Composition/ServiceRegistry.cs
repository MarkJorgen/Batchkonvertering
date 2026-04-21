using dk.gi.app.konto.beregnsatserlog.slet.Application.Contracts;
using dk.gi.app.konto.beregnsatserlog.slet.Application.Models;
using dk.gi.app.konto.beregnsatserlog.slet.Application.Services;
using dk.gi.app.konto.beregnsatserlog.slet.Infrastructure.Crm;
using Microsoft.Extensions.Logging;

namespace dk.gi.app.konto.beregnsatserlog.slet.Infrastructure.Composition
{
    public sealed class ServiceRegistry
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly SletBeregnSatserLogSettings _settings;

        public ServiceRegistry(ILoggerFactory loggerFactory, SletBeregnSatserLogSettings settings)
        {
            _loggerFactory = loggerFactory;
            _settings = settings;
        }

        public SletBeregnSatserLogOrchestrator CreateOrchestrator()
        {
            var connectionFactory = new DataverseConnectionFactory(_settings);
            IBeregnSatserLogRepository repository = new DataverseBeregnSatserLogRepository(connectionFactory, _loggerFactory.CreateLogger<DataverseBeregnSatserLogRepository>());
            IConnectivityVerifier verifier = new DataverseConnectivityVerifier(connectionFactory, _loggerFactory.CreateLogger<DataverseConnectivityVerifier>());

            return new SletBeregnSatserLogOrchestrator(
                repository,
                verifier,
                _loggerFactory.CreateLogger<SletBeregnSatserLogOrchestrator>());
        }
    }
}
