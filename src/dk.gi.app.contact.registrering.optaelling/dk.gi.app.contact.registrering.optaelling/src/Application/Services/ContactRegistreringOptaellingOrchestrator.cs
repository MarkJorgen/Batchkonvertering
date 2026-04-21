using System;
using dk.gi.app.contact.registrering.optaelling.Application.Abstractions;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Notifications;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Runtime;
using Gi.Batch.Shared.Execution;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.registrering.optaelling.Application.Services
{
    public sealed class ContactRegistreringOptaellingOrchestrator : Gi.Batch.Shared.Runtime.IJobOrchestrator
    {
        private readonly ContactRegistreringOptaellingSettings _settings;
        private readonly ContactRegistreringOptaellingRequest _request;
        private readonly SingleInstanceGuard _singleInstanceGuard;
        private readonly IContactRegistreringOptaellingGateway _gateway;
        private readonly IContactRegistreringCrmConnectionVerifier _crmConnectionVerifier;
        private readonly FailureNotificationService _failureNotificationService;
        private readonly IJobLogger _logger;

        public ContactRegistreringOptaellingOrchestrator(
            ContactRegistreringOptaellingSettings settings,
            ContactRegistreringOptaellingRequest request,
            SingleInstanceGuard singleInstanceGuard,
            IContactRegistreringOptaellingGateway gateway,
            IContactRegistreringCrmConnectionVerifier crmConnectionVerifier,
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
            _logger.Info("Contact registrering optælling starter.");

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

                if (summary.Success == false)
                {
                    _logger.Error(summary.Message, null);
                    return JobExecutionResult.Fail(_settings.VerifyCrmOnly ? 301 : 300, summary.Message);
                }

                string registreringText = _request.RegistreringId.HasValue
                    ? $"registreringid={_request.RegistreringId.Value}"
                    : "registreringid=<alle>";

                string message = _settings.VerifyCrmOnly
                    ? $"VERIFYCRM gennemført via {summary.Source}. Mode={_settings.Mode}. {summary.Message}"
                    : $"Workflow gennemført via {summary.Source}. " +
                      $"Lukkede treklip/ejerregistreringer={summary.ClosedExpiredTreklipOwnerRegistrations}. " +
                      $"Dannede kontaktjobs={summary.CreatedJobsForContacts}. " +
                      $"Mode={_settings.Mode}. {registreringText}. {summary.Message}";

                _logger.Info(message);
                return JobExecutionResult.Ok(message);
            }
            catch (Exception ex)
            {
                _failureNotificationService.NotifyFailure(
                    "Contact registrering optælling fejlede",
                    "Pilotjobbet fejlede under execution.",
                    ex);

                _logger.Error("Pilotjobbet fejlede.", ex);
                return JobExecutionResult.Fail(500, ex.Message);
            }
            finally
            {
                _singleInstanceGuard.Release();
            }
        }
    }
}
