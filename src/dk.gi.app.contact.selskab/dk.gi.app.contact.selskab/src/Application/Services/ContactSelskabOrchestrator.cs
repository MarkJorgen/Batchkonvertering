using System;
using dk.gi.app.contact.selskab.Application.Abstractions;
using dk.gi.app.contact.selskab.Application.Models;
using dk.gi.app.contact.selskab.Infrastructure.Notifications;
using dk.gi.app.contact.selskab.Infrastructure.Runtime;
using Gi.Batch.Shared.Execution;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.selskab.Application.Services
{
    public sealed class ContactSelskabOrchestrator : Gi.Batch.Shared.Runtime.IJobOrchestrator
    {
        private readonly ContactSelskabSettings _settings;
        private readonly ContactSelskabRequest _request;
        private readonly SingleInstanceGuard _singleInstanceGuard;
        private readonly IContactSelskabGateway _gateway;
        private readonly IContactSelskabCrmConnectionVerifier _crmConnectionVerifier;
        private readonly FailureNotificationService _failureNotificationService;
        private readonly IJobLogger _logger;

        public ContactSelskabOrchestrator(
            ContactSelskabSettings settings,
            ContactSelskabRequest request,
            SingleInstanceGuard singleInstanceGuard,
            IContactSelskabGateway gateway,
            IContactSelskabCrmConnectionVerifier crmConnectionVerifier,
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
            _logger.Info("Contact selskab starter.");

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
                    : $"Workflow gennemført via {summary.Source}. ObserveredeEjerrelationer={summary.ScannedOwnerRows}. Kandidater={summary.QualifiedCompanies}. Publiceret={summary.PublishedJobs}. Mode={_settings.Mode}. {scopeText}. {summary.Message}";

                _logger.Info(message);
                return JobExecutionResult.Ok(message);
            }
            catch (Exception ex)
            {
                _failureNotificationService.NotifyFailure(
                    "Contact selskab fejlede",
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
