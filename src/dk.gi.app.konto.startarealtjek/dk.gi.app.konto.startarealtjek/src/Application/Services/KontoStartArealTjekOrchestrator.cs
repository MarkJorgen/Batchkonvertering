using System;
using dk.gi.app.konto.startarealtjek.Application.Abstractions;
using dk.gi.app.konto.startarealtjek.Application.Models;
using dk.gi.app.konto.startarealtjek.Infrastructure.Notifications;
using dk.gi.app.konto.startarealtjek.Infrastructure.Runtime;
using Gi.Batch.Shared.Execution;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.startarealtjek.Application.Services
{
    public sealed class KontoStartArealTjekOrchestrator : Gi.Batch.Shared.Runtime.IJobOrchestrator
    {
        private readonly KontoStartArealTjekSettings _settings;
        private readonly KontoStartArealTjekRequest _request;
        private readonly SingleInstanceGuard _singleInstanceGuard;
        private readonly IKontoStartArealTjekGateway _gateway;
        private readonly IKontoStartArealTjekCrmConnectionVerifier _crmConnectionVerifier;
        private readonly FailureNotificationService _failureNotificationService;
        private readonly IJobLogger _logger;

        public KontoStartArealTjekOrchestrator(
            KontoStartArealTjekSettings settings,
            KontoStartArealTjekRequest request,
            SingleInstanceGuard singleInstanceGuard,
            IKontoStartArealTjekGateway gateway,
            IKontoStartArealTjekCrmConnectionVerifier crmConnectionVerifier,
            FailureNotificationService failureNotificationService,
            IJobLogger logger)
        {
            _settings = settings;
            _request = request;
            _singleInstanceGuard = singleInstanceGuard;
            _gateway = gateway;
            _crmConnectionVerifier = crmConnectionVerifier;
            _failureNotificationService = failureNotificationService;
            _logger = logger;
        }

        public JobExecutionResult Run()
        {
            _logger.Info("Konto startarealtjek starter.");

            if (_singleInstanceGuard.TryAcquire() == false)
            {
                const string alreadyRunningMessage = "Applikationen kører allerede. Afslutter uden fejl.";
                _logger.Warning(alreadyRunningMessage);
                return JobExecutionResult.Ok(alreadyRunningMessage);
            }

            try
            {
                var summary = _settings.VerifyCrmOnly
                    ? _crmConnectionVerifier.Verify()
                    : _gateway.Execute(_request);

                if (!summary.Success)
                {
                    _logger.Error(summary.Message);
                    return JobExecutionResult.Fail(_settings.VerifyCrmOnly ? 321 : 320, summary.Message);
                }

                string scopeText = string.IsNullOrWhiteSpace(_request.KontoNr)
                    ? "kontonr=<alle>"
                    : "kontonr=" + _request.KontoNr;

                string message = _settings.VerifyCrmOnly
                    ? $"VERIFYCRM gennemført via {summary.Source}. Mode={_settings.Mode}. {summary.Message}"
                    : $"Workflow gennemført via {summary.Source}. Scannet={summary.ScannedAccounts}. Emner={summary.SubjectAccounts}. IkkeEmner={summary.NonSubjectAccounts}. Publiceret={summary.PublishedJobs}. Mode={_settings.Mode}. {scopeText}. {summary.Message}";

                _logger.Info(message);
                return JobExecutionResult.Ok(message);
            }
            catch (Exception ex)
            {
                _failureNotificationService.NotifyFailure(
                    "Konto startarealtjek fejlede",
                    "Jobbet fejlede under execution.",
                    ex);

                _logger.Error("Jobbet fejlede.", ex);
                return JobExecutionResult.Fail(500, ex.Message);
            }
            finally
            {
                _singleInstanceGuard.Release();
            }
        }
    }
}
