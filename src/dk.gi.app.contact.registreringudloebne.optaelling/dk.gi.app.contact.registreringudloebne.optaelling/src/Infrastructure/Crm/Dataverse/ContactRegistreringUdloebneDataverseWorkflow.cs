using System;
using System.Linq;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Abstractions;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Crm
{
    public sealed class ContactRegistreringUdloebneDataverseWorkflow : IContactRegistreringUdloebneWorkflow
    {
        private readonly IRunoutRegistreringScanClientFactory _clientFactory;
        private readonly IRunoutRegistreringJobPublisher _jobPublisher;
        private readonly IJobLogger _logger;

        public ContactRegistreringUdloebneDataverseWorkflow(IRunoutRegistreringScanClientFactory clientFactory, IRunoutRegistreringJobPublisher jobPublisher, IJobLogger logger)
        {
            _clientFactory = clientFactory;
            _jobPublisher = jobPublisher;
            _logger = logger;
        }

        public ContactRegistreringUdloebneExecutionSummary Execute(Guid? registreringId)
        {
            try
            {
                using (var client = _clientFactory.Create())
                {
                    var candidates = client.FindCandidates(registreringId);
                    var treklipIds = candidates.Where(c => c.TreklipId.HasValue).Select(c => c.TreklipId.Value).Distinct().ToList();
                    var closedTreklipIds = client.GetClosedTreklipIds(treklipIds);
                    var resolvedServiceBusSettings = client.ResolveServiceBusSettings();

                    if (resolvedServiceBusSettings != null && resolvedServiceBusSettings.IsConfigured)
                    {
                        _logger.Info("Service Bus resolved via " + resolvedServiceBusSettings.Source + ".");
                    }
                    else
                    {
                        _logger.Warning("Service Bus kunne ikke resolves fuldt ud via lokale settings eller CRM config_configurationsetting.");
                    }

                    var publishResult = _jobPublisher.Publish(candidates, closedTreklipIds, resolvedServiceBusSettings);

                    return new ContactRegistreringUdloebneExecutionSummary(
                        publishResult.Success,
                        candidates.Count,
                        publishResult.PublishedCount,
                        publishResult.Message,
                        "local dataverse + servicebus adapter");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Runout workflow kastede exception. " + ex.Message);
                return new ContactRegistreringUdloebneExecutionSummary(false, 0, 0, "Runout workflow kastede exception: " + ex.Message, ex.GetType().FullName);
            }
        }
    }
}
