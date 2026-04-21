using System;
using dk.gi.app.contact.lassox.ophoer.Application.Abstractions;
using dk.gi.app.contact.lassox.ophoer.Application.Models;
using dk.gi.app.contact.lassox.ophoer.Infrastructure.Notifications;
using dk.gi.app.contact.lassox.ophoer.Infrastructure.Runtime;
using Gi.Batch.Shared.Execution;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.lassox.ophoer.Application.Services
{
    public sealed class LassoXOphoerOrchestrator : Gi.Batch.Shared.Runtime.IJobOrchestrator
    {
        private readonly LassoXOphoerSettings _settings;
        private readonly LassoXOphoerRequest _request;
        private readonly SingleInstanceGuard _singleInstanceGuard;
        private readonly ILassoXOphoerGateway _gateway;
        private readonly ILassoXOphoerCrmConnectionVerifier _crmConnectionVerifier;
        private readonly FailureNotificationService _failureNotificationService;
        private readonly IJobLogger _logger;

        public LassoXOphoerOrchestrator(
            LassoXOphoerSettings settings,
            LassoXOphoerRequest request,
            SingleInstanceGuard singleInstanceGuard,
            ILassoXOphoerGateway gateway,
            ILassoXOphoerCrmConnectionVerifier crmConnectionVerifier,
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
            _logger.Info("Contact LassoX ophør starter.");

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

                string scopeText = _request.ContactId.HasValue
                    ? "contactid=" + _request.ContactId.Value
                    : "contactid=<alle>";

                string message = _settings.VerifyCrmOnly
                    ? $"VERIFYCRM gennemført via {summary.Source}. Mode={_settings.Mode}. {summary.Message}"
                    : $"Workflow gennemført via {summary.Source}. Scannet={summary.ScannedContacts}. Til afmelding={summary.ContactsMarkedForUnsubscribe}. Opdateret={summary.UpdatedContacts}. Mode={_settings.Mode}. {scopeText}. {summary.Message}";

                _logger.Info(message);
                return JobExecutionResult.Ok(message);
            }
            catch (Exception ex)
            {
                _failureNotificationService.NotifyFailure(
                    "Contact LassoX ophør fejlede",
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
