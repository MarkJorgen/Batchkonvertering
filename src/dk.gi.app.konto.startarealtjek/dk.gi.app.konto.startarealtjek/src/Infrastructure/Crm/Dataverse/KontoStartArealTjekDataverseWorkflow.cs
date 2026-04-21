using System;
using dk.gi.app.konto.startarealtjek.Application.Abstractions;
using dk.gi.app.konto.startarealtjek.Application.Models;
using dk.gi.app.konto.startarealtjek.Application.Services;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.startarealtjek.Infrastructure.Crm
{
    public sealed class KontoStartArealTjekDataverseWorkflow : IKontoStartArealTjekWorkflow
    {
        private readonly KontoStartArealTjekSettings _settings;
        private readonly IKontoStartArealTjekScanClientFactory _clientFactory;
        private readonly KontoStartArealTjekSelectionEngine _selectionEngine;
        private readonly IKontoStartArealTjekJobPublisher _jobPublisher;
        private readonly IJobLogger _logger;

        public KontoStartArealTjekDataverseWorkflow(
            KontoStartArealTjekSettings settings,
            IKontoStartArealTjekScanClientFactory clientFactory,
            KontoStartArealTjekSelectionEngine selectionEngine,
            IKontoStartArealTjekJobPublisher jobPublisher,
            IJobLogger logger)
        {
            _settings = settings;
            _clientFactory = clientFactory;
            _selectionEngine = selectionEngine;
            _jobPublisher = jobPublisher;
            _logger = logger;
        }

        public KontoStartArealTjekExecutionSummary Execute(KontoStartArealTjekRequest request)
        {
            try
            {
                using (var client = _clientFactory.Create())
                {
                    var batchSettings = client.ResolveBatchSettings();
                    var assessments = client.AssessAccounts(request, batchSettings);
                    var candidates = _selectionEngine.SelectCandidates(assessments, batchSettings);
                    var resolvedServiceBusSettings = client.ResolveServiceBusSettings();

                    if (resolvedServiceBusSettings != null && resolvedServiceBusSettings.IsConfigured)
                        _logger.Info("Service Bus resolved via " + resolvedServiceBusSettings.Source + ".");
                    else
                        _logger.Warning("Service Bus kunne ikke resolves fuldt ud via lokale settings eller CRM config_configurationsetting.");

                    int scanned = assessments.Count;
                    int subjects = 0;
                    foreach (var assessment in assessments)
                    {
                        if (assessment != null && assessment.ShouldBeSubject)
                            subjects++;
                    }
                    int nonSubjects = scanned - subjects;

                    if (_settings.DryRun)
                    {
                        string dryRunMessage = "DRYRUN gennemført. ap_emneforarealtjek blev ikke opdateret, og arealtjek-job blev ikke publiceret.";
                        return new KontoStartArealTjekExecutionSummary(true, scanned, subjects, nonSubjects, 0, dryRunMessage + " BatchSettingsKilde=" + batchSettings.Source + ".", "local dataverse sdk");
                    }

                    _logger.Info("Starter write-fase for konto.startarealtjek. KandidaterTilPublicering=" + candidates.Count + ".");
                    client.ApplySubjectFlagUpdates(assessments);
                    _logger.Info("Write-fase for ap_emneforarealtjek er gennemført. Starter Service Bus-publicering af " + candidates.Count + " job(s).");
                    var publishResult = _jobPublisher.Publish(candidates, resolvedServiceBusSettings);

                    return new KontoStartArealTjekExecutionSummary(
                        publishResult.Success,
                        scanned,
                        subjects,
                        nonSubjects,
                        publishResult.PublishedCount,
                        publishResult.Message + " BatchSettingsKilde=" + batchSettings.Source + ".",
                        "local dataverse + servicebus adapter");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Konto startarealtjek workflow kastede exception. " + ex.Message);
                return new KontoStartArealTjekExecutionSummary(false, 0, 0, 0, 0, "Konto startarealtjek workflow kastede exception: " + ex.Message, ex.GetType().FullName);
            }
        }
    }
}
