using dk.gi.app.konto.regnskab.slet.Application.Contracts;
using dk.gi.app.konto.regnskab.slet.Application.Models;
using dk.gi.app.konto.regnskab.slet.Application.Services;
using dk.gi.app.konto.regnskab.slet.Infrastructure.Crm;
using dk.gi.app.konto.regnskab.slet.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

namespace dk.gi.app.konto.regnskab.slet.Infrastructure.Composition
{
    public sealed class ServiceRegistry
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly RegnskabSletSettings _settings;

        public ServiceRegistry(ILoggerFactory loggerFactory, RegnskabSletSettings settings)
        {
            _loggerFactory = loggerFactory;
            _settings = settings;
        }

        public RegnskabSletOrchestrator CreateOrchestrator()
        {
            var connectionFactory = new DataverseConnectionFactory(_settings);
            IRegnskabSletRepository repository = new DataverseRegnskabSletRepository(connectionFactory, _settings, _loggerFactory.CreateLogger<DataverseRegnskabSletRepository>());
            IRegnskabSletPublisher publisher = new RegnskabSletPublisher(_settings, new RegnskabSletServiceBusSender(_settings, _loggerFactory.CreateLogger<RegnskabSletServiceBusSender>()), _loggerFactory.CreateLogger<RegnskabSletPublisher>());
            IConnectivityVerifier verifier = new DataverseConnectivityVerifier(connectionFactory, _loggerFactory.CreateLogger<DataverseConnectivityVerifier>());
            return new RegnskabSletOrchestrator(repository, publisher, verifier, _loggerFactory.CreateLogger<RegnskabSletOrchestrator>());
        }
    }
}
