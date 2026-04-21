using System;
using System.Linq;
using dk.gi.app.konto.afslutarealsager.Application.Abstractions;
using dk.gi.app.konto.afslutarealsager.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Crm.Dataverse
{
    public sealed class KontoAfslutArealSagerDataverseWorkflow : IKontoAfslutArealSagerWorkflow
    {
        private readonly KontoAfslutArealSagerSettings _settings;
        private readonly IKontoAfslutArealSagerScanClientFactory _scanClientFactory;
        private readonly ILetterGenerator _letterGenerator;
        private readonly IKontoAfslutArealSagerCloseoutPublisher _closeoutPublisher;
        private readonly IKontoAfslutArealSagerArealSumPublisher _arealSumPublisher;
        private readonly IJobLogger _logger;

        public KontoAfslutArealSagerDataverseWorkflow(
            KontoAfslutArealSagerSettings settings,
            IKontoAfslutArealSagerScanClientFactory scanClientFactory,
            ILetterGenerator letterGenerator,
            IKontoAfslutArealSagerCloseoutPublisher closeoutPublisher,
            IKontoAfslutArealSagerArealSumPublisher arealSumPublisher,
            IJobLogger logger)
        {
            _settings = settings;
            _scanClientFactory = scanClientFactory;
            _letterGenerator = letterGenerator;
            _closeoutPublisher = closeoutPublisher;
            _arealSumPublisher = arealSumPublisher;
            _logger = logger;
        }

        public KontoAfslutArealSagerExecutionSummary Execute(KontoAfslutArealSagerRequest request)
        {
            if (_settings.RunMode && !_settings.AllowPartialRun && !_settings.EnableDiscoveryRun)
            {
                return KontoAfslutArealSagerExecutionSummary.Fail(
                    "Dataverse",
                    "RUN er fortsat blokeret uden AllowPartialRun=true. Denne fase 6-leverance kan nu oprette aktivitet, vedhæfte PDF som note, valgfrit publicere closeout-job til Service Bus, valgfrit lukke incident direkte i Dataverse og valgfrit køre et lokalt areal carry-forward-seam med AREALSUM2KONTO-job, men den endelige GI-ækvivalente areal-closeout-vej er endnu ikke porteret.",
                    partialRunBlocked: true);
            }

            using (var client = _scanClientFactory.Create())
            {
                client.EnsureConnection();

                if (_settings.EnableDiscoveryRun)
                {
                    var discoveredCases = client.DiscoverCases(request, _settings.DiscoveryLimit);
                    var usableCases = discoveredCases.Where(x => x.IsUsableForLetter).ToList();
                    int skippedDiscoveredCases = discoveredCases.Count - usableCases.Count;
                    return KontoAfslutArealSagerExecutionSummary.Ok(
                        "Dataverse",
                        "Discovery gennemført. Kandidater er logget read-only til videre teknisk verificering; ingen CRM-write, ingen upload, ingen queue, ingen digital post og ingen closeout blev udført.",
                        discoveredCases.Count,
                        usableCases.Count,
                        0,
                        skippedDiscoveredCases);
                }

                var cases = client.GetOpenCases(request);
                var letterCandidates = cases.Where(x => x.IsUsableForLetter).ToList();
                int skipped = cases.Count - letterCandidates.Count;
                int generated = 0;
                int createdActivities = 0;
                int uploadedLetters = 0;
                int completedActivities = 0;
                int publishedCloseoutJobs = 0;
                int closedIncidents = 0;
                int closedAreas = 0;
                int createdAreas = 0;
                int deletedZeroRegnskaber = 0;
                int publishedArealSumJobs = 0;
                int stagedDigitalPosts = 0;
                ResolvedServiceBusSettings resolvedServiceBusSettings = null;

                if (_settings.RunMode && _settings.AllowPartialRun && (_settings.EnableCloseoutQueueRun || _settings.EnableArealSumQueueRun))
                {
                    resolvedServiceBusSettings = client.ResolveServiceBusSettings();
                    if (resolvedServiceBusSettings != null && resolvedServiceBusSettings.IsConfigured)
                    {
                        _logger.Info("Service Bus resolved via " + resolvedServiceBusSettings.Source + ".");
                    }
                    else
                    {
                        _logger.Warning("Service Bus-toggles er aktive, men Service Bus-settings kunne ikke resolves endnu. Kilde=" + (resolvedServiceBusSettings != null ? resolvedServiceBusSettings.Source : "none"));
                    }
                }

                if (cases.Count == 0)
                {
                    _logger.Info("Ingen åbne incidents matchede det aktuelle filter i den nye Dataverse-vej.");
                }

                foreach (var candidate in letterCandidates)
                {
                    var mergeData = KontoAfslutArealSagerLetterMergeData.Create(candidate);
                    byte[] pdf = _letterGenerator.GeneratePdf(mergeData);
                    if (pdf == null || pdf.Length == 0)
                    {
                        _logger.Warning("Springer sag " + candidate.CaseNumber + " over, fordi PDF-generering gav tomt resultat.");
                        continue;
                    }

                    generated++;
                    _logger.Info("Genererede brev i hukommelsen for sag " + candidate.CaseNumber + ". Bytes=" + pdf.Length);

                    if (_settings.RunMode && _settings.AllowPartialRun)
                    {
                        Guid activityId = client.CreateLetterActivity(candidate, request);
                        createdActivities++;
                        client.UploadLetterToActivity(activityId, candidate, pdf);
                        uploadedLetters++;

                        if (_settings.TilladSendTilDigitalPost && _settings.EnableDigitalPostStubRun)
                        {
                            client.StageDigitalPostNote(activityId, candidate, pdf, "Ændring af areal");
                            stagedDigitalPosts++;
                        }
                        else if (_settings.TilladSendTilDigitalPost)
                        {
                            _logger.Warning("TilladSendTilDigitalPost=true, men EnableDigitalPostStubRun=false. Digital post bliver derfor endnu ikke afsendt eller staged for sag " + candidate.CaseNumber + ".");
                        }

                        client.CompleteActivity(activityId);
                        completedActivities++;
                        _logger.Info("Partial RUN write-fase gennemført for sag " + candidate.CaseNumber + ". Aktivitet=" + activityId.ToString("D"));

                        if (_settings.EnableCloseoutQueueRun)
                        {
                            if (!_closeoutPublisher.Publish(candidate, resolvedServiceBusSettings, 30))
                            {
                                throw new InvalidOperationException("Closeout-job kunne ikke publiceres for sag " + candidate.CaseNumber + ".");
                            }

                            publishedCloseoutJobs++;
                            _logger.Info("Closeout-job publiceret for sag " + candidate.CaseNumber + ".");
                        }

                        if (_settings.EnableDirectIncidentCloseoutRun)
                        {
                            client.CloseIncident(candidate, _settings.DirectIncidentCloseStatusCode, "Luk areal check", "Partial RUN fase 7 direkte incident-closeout fra dk.gi.app.konto.afslutarealsager.");
                            closedIncidents++;
                        }

                        if (_settings.EnableCarryForwardArealRun)
                        {
                            var arealResult = client.CarryForwardOpenArea(candidate, _settings.EnableDeleteZeroRegnskabRun);
                            if (arealResult.Attempted)
                            {
                                closedAreas += arealResult.ClosedExistingArea ? 1 : 0;
                                createdAreas += arealResult.CreatedNewArea ? 1 : 0;
                                deletedZeroRegnskaber += arealResult.DeletedZeroRegnskab ? 1 : 0;
                                _logger.Info("Areal carry-forward for konto " + candidate.AccountNumber + ": " + arealResult.Message);

                                if (_settings.EnableArealSumQueueRun && !string.IsNullOrWhiteSpace(arealResult.NewAreaId))
                                {
                                    if (!_arealSumPublisher.Publish(candidate, arealResult.NewAreaId, resolvedServiceBusSettings, 60))
                                    {
                                        throw new InvalidOperationException("AREALSUM2KONTO-job kunne ikke publiceres for konto " + candidate.AccountNumber + ".");
                                    }

                                    publishedArealSumJobs++;
                                }
                            }
                            else
                            {
                                _logger.Info("Areal carry-forward blev skippet for konto " + candidate.AccountNumber + ": " + arealResult.Message);
                            }
                        }
                    }
                }

                string message;
                if (_settings.DryRun)
                {
                    message = "DRYRUN gennemført. Breve blev kun genereret i hukommelsen; ingen CRM-write, ingen upload, ingen digital post, ingen sagslukning og ingen areal-closeout.";
                }
                else if (_settings.EnableCloseoutQueueRun || _settings.EnableDirectIncidentCloseoutRun || _settings.EnableCarryForwardArealRun || _settings.EnableArealSumQueueRun)
                {
                    message = "Partial RUN fase 7 gennemført. Oprettede aktiviteter, vedhæftede PDF som note og lukkede aktiviteter";
                    if (_settings.EnableCloseoutQueueRun)
                    {
                        message += ", publicerede closeout-job til Service Bus";
                    }
                    if (_settings.EnableDirectIncidentCloseoutRun)
                    {
                        message += ", og lukkede incident direkte i Dataverse";
                    }
                    if (_settings.EnableCarryForwardArealRun)
                    {
                        message += ", og kørte lokalt areal carry-forward";
                    }
                    if (_settings.EnableArealSumQueueRun)
                    {
                        message += ", inkl. AREALSUM2KONTO-job";
                    }
                    if (_settings.TilladSendTilDigitalPost && _settings.EnableDigitalPostStubRun)
                    {
                        message += ", og staged digital post som note";
                    }
                    message += ". GI-ækvivalent areal-closeout er fortsat ikke porteret i denne leverance.";
                }
                else
                {
                    message = "Partial RUN fase 7 basisvej gennemført. Oprettede aktiviteter, vedhæftede PDF som note og lukkede aktiviteter. Valgfri closeout/incident-closeout/areal-carry-forward/digital-post-staging blev kørt bag eksplicitte toggles. GI-ækvivalent areal-closeout er fortsat ikke porteret i denne leverance.";
                }

                return KontoAfslutArealSagerExecutionSummary.Ok(
                    "Dataverse",
                    message,
                    cases.Count,
                    letterCandidates.Count,
                    generated,
                    skipped,
                    createdActivities,
                    uploadedLetters,
                    completedActivities,
                    publishedCloseoutJobs,
                    closedIncidents,
                    closedAreas,
                    createdAreas,
                    deletedZeroRegnskaber,
                    publishedArealSumJobs,
                    stagedDigitalPosts);
            }
        }
    }
}
