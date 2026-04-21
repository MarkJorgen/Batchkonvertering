using System;
using dk.gi.app.ejendom.tjekejerskifte.Application.Abstractions;
using dk.gi.app.ejendom.tjekejerskifte.Application.Models;
using dk.gi.app.ejendom.tjekejerskifte.Infrastructure.Notifications;
using dk.gi.app.ejendom.tjekejerskifte.Infrastructure.Runtime;
using Gi.Batch.Shared.Execution;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.ejendom.tjekejerskifte.Application.Services
{
    public sealed class EjendomTjekEjerskifteOrchestrator : Gi.Batch.Shared.Runtime.IJobOrchestrator
    {
        private readonly EjendomTjekEjerskifteSettings _settings; private readonly EjendomTjekEjerskifteRequest _request; private readonly SingleInstanceGuard _singleInstanceGuard; private readonly IEjendomTjekEjerskifteGateway _gateway; private readonly IEjendomTjekEjerskifteCrmConnectionVerifier _crmConnectionVerifier; private readonly FailureNotificationService _failureNotificationService; private readonly IJobLogger _logger;
        public EjendomTjekEjerskifteOrchestrator(EjendomTjekEjerskifteSettings settings, EjendomTjekEjerskifteRequest request, SingleInstanceGuard singleInstanceGuard, IEjendomTjekEjerskifteGateway gateway, IEjendomTjekEjerskifteCrmConnectionVerifier crmConnectionVerifier, FailureNotificationService failureNotificationService, IJobLogger logger)
        { _settings = settings; _request = request; _singleInstanceGuard = singleInstanceGuard; _gateway = gateway; _crmConnectionVerifier = crmConnectionVerifier; _failureNotificationService = failureNotificationService; _logger = logger; }
        public JobExecutionResult Run()
        {
            _logger.Info("Ejendom tjekejerskifte starter.");
            if (_singleInstanceGuard.TryAcquire() == false) { const string m = "Applikationen kører allerede. Afslutter uden fejl."; _logger.Warning(m); return JobExecutionResult.Ok(m); }
            try
            {
                var summary = _settings.VerifyCrmOnly ? _crmConnectionVerifier.Verify() : _gateway.Execute(_request);
                if (!summary.Success) { _logger.Error(summary.Message); return JobExecutionResult.Fail(_settings.VerifyCrmOnly ? 321 : 320, summary.Message); }
                string message = _settings.VerifyCrmOnly ? $"VERIFYCRM gennemført via {summary.Source}. Mode={_settings.Mode}. {summary.Message}" : $"Workflow gennemført via {summary.Source}. Ejendomme={summary.ScannedProperties}. Publiceret={summary.PublishedJobs}. Uden BFEnummer={summary.SkippedWithoutBfe}. Mode={_settings.Mode}. {summary.Message}";
                _logger.Info(message); return JobExecutionResult.Ok(message);
            }
            catch (Exception ex)
            {
                _failureNotificationService.NotifyFailure("Ejendom tjekejerskifte fejlede", "Jobbet fejlede under execution.", ex);
                _logger.Error("Jobbet fejlede.", ex); return JobExecutionResult.Fail(500, ex.Message);
            }
            finally { _singleInstanceGuard.Release(); }
        }
    }
}
