using dk.gi.app.konto.satser.opret.Application.Contracts;
using dk.gi.app.konto.satser.opret.Application.Models;
using dk.gi.app.konto.satser.opret.Application.Services;
using Microsoft.Extensions.Logging;

namespace dk.gi.app.konto.satser.opret.Infrastructure.Composition
{
    public sealed class ServiceRegistry
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly OpretSatserSettings _settings;

        public ServiceRegistry(ILoggerFactory loggerFactory, OpretSatserSettings settings)
        {
            _loggerFactory = loggerFactory;
            _settings = settings;
        }

        public OpretSatserOrchestrator CreateOrchestrator(IOpretSatserRepository repository, IConnectivityVerifier verifier)
        {
            return new OpretSatserOrchestrator(
                repository,
                verifier,
                _loggerFactory.CreateLogger<OpretSatserOrchestrator>());
        }
    }
}
