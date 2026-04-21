using System;
using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.regnskab.slet.Application.Contracts;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;

namespace dk.gi.app.konto.regnskab.slet.Infrastructure.Crm
{
    public sealed class DataverseConnectivityVerifier : IConnectivityVerifier
    {
        private readonly DataverseConnectionFactory _connectionFactory;
        private readonly ILogger _logger;

        public DataverseConnectivityVerifier(DataverseConnectionFactory connectionFactory, ILogger logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public Task VerifyAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var client = _connectionFactory.CreateClient())
            {
                if (!client.IsReady) throw new InvalidOperationException(client.LastError ?? "Dataverse-klienten er ikke klar.");
                client.Execute(new WhoAmIRequest());
                _logger.LogInformation("Dataverse-connectivity verificeret for konto.regnskab.slet.");
            }
            return Task.CompletedTask;
        }
    }
}
