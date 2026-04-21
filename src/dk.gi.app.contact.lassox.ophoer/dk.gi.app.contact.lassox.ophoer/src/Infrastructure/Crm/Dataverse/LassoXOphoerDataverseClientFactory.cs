using System;
using dk.gi.app.contact.lassox.ophoer.Application.Abstractions;
using dk.gi.app.contact.lassox.ophoer.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.lassox.ophoer.Infrastructure.Crm
{
    public sealed class LassoXOphoerDataverseClientFactory : ILassoXOphoerScanClientFactory
    {
        private readonly LassoXOphoerSettings _settings;
        private readonly IJobLogger _logger;

        public LassoXOphoerDataverseClientFactory(LassoXOphoerSettings settings, IJobLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public ILassoXOphoerScanClient Create()
        {
            _logger.Info("Opretter Dataverse klient. Sanitized connection string: " + CrmConnectionStringFactory.CreateSanitized(_settings));
            DateTime started = DateTime.UtcNow;
            var client = new LassoXOphoerDataverseClient(
                CrmConnectionStringFactory.Create(_settings),
                _settings.TimeOutMinutes,
                message => _logger.Info(message));
            _logger.Info("Dataverse klient oprettet efter " + (DateTime.UtcNow - started).TotalSeconds.ToString("0.0") + " sek.");
            return client;
        }
    }
}
