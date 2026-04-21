using System;
using dk.gi.app.konto.afslutarealsager.Application.Abstractions;
using dk.gi.app.konto.afslutarealsager.Application.Models;
using dk.gi.app.konto.afslutarealsager.Infrastructure.Notifications;
using dk.gi.app.konto.afslutarealsager.Infrastructure.Runtime;
using Gi.Batch.Shared.Execution;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.afslutarealsager.Application.Services
{
    public sealed class KontoAfslutArealSagerOrchestrator : Gi.Batch.Shared.Runtime.IJobOrchestrator
    {
        private readonly KontoAfslutArealSagerSettings _settings;
        private readonly KontoAfslutArealSagerRequest _request;
        private readonly SingleInstanceGuard _singleInstanceGuard;
        private readonly IKontoAfslutArealSagerGateway _gateway;
        private readonly IKontoAfslutArealSagerCrmConnectionVerifier _crmConnectionVerifier;
        private readonly FailureNotificationService _failureNotificationService;
        private readonly IJobLogger _logger;

        public KontoAfslutArealSagerOrchestrator(
            KontoAfslutArealSagerSettings settings,
            KontoAfslutArealSagerRequest request,
            SingleInstanceGuard singleInstanceGuard,
            IKontoAfslutArealSagerGateway gateway,
            IKontoAfslutArealSagerCrmConnectionVerifier crmConnectionVerifier,
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
            _logger.Info("Konto afslutarealsager starter.");

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
                    return JobExecutionResult.Fail(summary.PartialRunBlocked ? 331 : 330, summary.Message);
                }

                string message = _settings.VerifyCrmOnly
                    ? $"VERIFYCRM gennemført via {summary.Source}. Mode={_settings.Mode}. {summary.Message}"
                    : $"Workflow gennemført via {summary.Source}. Scannet={summary.ScannedCases}. Brevkandidater={summary.LetterCandidates}. GenereredeBreve={summary.GeneratedLetters}. Skippede={summary.SkippedCases}. OprettedeAktiviteter={summary.CreatedActivities}. UploadedeBreve={summary.UploadedLetters}. LukkedeAktiviteter={summary.CompletedActivities}. PubliceredeCloseoutJobs={summary.PublishedCloseoutJobs}. LukkedeIncidents={summary.ClosedIncidents}. LukkedeArealer={summary.ClosedAreas}. OprettedeArealer={summary.CreatedAreas}. Slettede0Regnskaber={summary.DeletedZeroRegnskaber}. PubliceredeArealSumJobs={summary.PublishedArealSumJobs}. StagedDigitalPosts={summary.StagedDigitalPosts}. Mode={_settings.Mode}. {summary.Message}";

                _logger.Info(message);
                return JobExecutionResult.Ok(message);
            }
            catch (Exception ex)
            {
                _failureNotificationService.NotifyFailure(
                    "Konto afslutarealsager fejlede",
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
