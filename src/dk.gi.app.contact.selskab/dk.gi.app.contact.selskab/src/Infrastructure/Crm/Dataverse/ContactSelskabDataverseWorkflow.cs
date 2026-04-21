using System;
using dk.gi.app.contact.selskab.Application.Abstractions;
using dk.gi.app.contact.selskab.Application.Models;
using dk.gi.app.contact.selskab.Application.Services;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.selskab.Infrastructure.Crm
{
    public sealed class ContactSelskabDataverseWorkflow : IContactSelskabWorkflow
    {
        private readonly ContactSelskabSettings _settings;
        private readonly IContactSelskabScanClientFactory _clientFactory;
        private readonly ContactSelskabSelectionEngine _selectionEngine;
        private readonly IContactSelskabJobPublisher _jobPublisher;
        private readonly IJobLogger _logger;

        public ContactSelskabDataverseWorkflow(
            ContactSelskabSettings settings,
            IContactSelskabScanClientFactory clientFactory,
            ContactSelskabSelectionEngine selectionEngine,
            IContactSelskabJobPublisher jobPublisher,
            IJobLogger logger)
        {
            _settings = settings;
            _clientFactory = clientFactory;
            _selectionEngine = selectionEngine;
            _jobPublisher = jobPublisher;
            _logger = logger;
        }

        public ContactSelskabExecutionSummary Execute(ContactSelskabRequest request)
        {
            try
            {
                using (var client = _clientFactory.Create())
                {
                    var observations = client.GetOwnerObservations(request.ContactId);
                    var candidates = _selectionEngine.SelectCandidates(observations);
                    var resolvedServiceBusSettings = client.ResolveServiceBusSettings();

                    if (resolvedServiceBusSettings != null && resolvedServiceBusSettings.IsConfigured)
                    {
                        _logger.Info("Service Bus resolved via " + resolvedServiceBusSettings.Source + ".");
                    }
                    else
                    {
                        _logger.Warning("Service Bus kunne ikke resolves fuldt ud via lokale settings eller CRM config_configurationsetting.");
                    }

                    if (_settings.DryRun)
                    {
                        string dryRunMessage = "DRYRUN gennemført. Kvalificerede selskaber blev ikke publiceret til Service Bus.";
                        return new ContactSelskabExecutionSummary(
                            true,
                            observations.Count,
                            candidates.Count,
                            0,
                            dryRunMessage,
                            "local dataverse sdk");
                    }

                    var publishResult = _jobPublisher.Publish(candidates, resolvedServiceBusSettings);

                    return new ContactSelskabExecutionSummary(
                        publishResult.Success,
                        observations.Count,
                        candidates.Count,
                        publishResult.PublishedCount,
                        publishResult.Message,
                        "local dataverse + servicebus adapter");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Contact selskab workflow kastede exception. " + ex.Message);
                return new ContactSelskabExecutionSummary(false, 0, 0, 0, "Contact selskab workflow kastede exception: " + ex.Message, ex.GetType().FullName);
            }
        }
    }
}
