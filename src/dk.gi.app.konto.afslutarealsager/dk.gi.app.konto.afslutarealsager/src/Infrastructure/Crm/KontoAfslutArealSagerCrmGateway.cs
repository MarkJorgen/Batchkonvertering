using dk.gi.app.konto.afslutarealsager.Application.Abstractions;
using dk.gi.app.konto.afslutarealsager.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Crm
{
    public sealed class KontoAfslutArealSagerCrmGateway : IKontoAfslutArealSagerGateway
    {
        private readonly KontoAfslutArealSagerSettings _settings;
        private readonly IKontoAfslutArealSagerWorkflow _workflow;
        private readonly IJobLogger _logger;

        public KontoAfslutArealSagerCrmGateway(KontoAfslutArealSagerSettings settings, IKontoAfslutArealSagerWorkflow workflow, IJobLogger logger)
        {
            _settings = settings;
            _workflow = workflow;
            _logger = logger;
        }

        public KontoAfslutArealSagerExecutionSummary Execute(KontoAfslutArealSagerRequest request)
        {
            if (_settings.EnableDiscoveryRun)
            {
                string discoveryMessage = "Discovery-mode aktiv. Kører read-only kandidatsøgning uden owner-filter for at finde teknisk verificerbare sager/konti.";
                if (_settings.RunMode)
                {
                    discoveryMessage += " RUN-mode er undertrykt til read-only, så der udføres ingen CRM-write, ingen queue, ingen digital post og ingen closeout i discovery.";
                }
                _logger.Warning(discoveryMessage);
            }
            else if (_settings.DryRun)
            {
                _logger.Info("DRYRUN-mode aktiv. Kører lokal Dataverse-scan og brevforberedelse uden opgaveoprettelse, upload, digital post, sagslukning eller areal-closeout.");
            }
            else if (_settings.AllowPartialRun)
            {
                string runMessage = "RUN-mode med AllowPartialRun=true. Denne fase 7-leverance opretter aktivitet, vedhæfter PDF som note og lukker aktiviteten";
                if (_settings.EnableCloseoutQueueRun)
                {
                    runMessage += ", kan publicere closeout-job til Service Bus";
                }
                if (_settings.EnableDirectIncidentCloseoutRun)
                {
                    runMessage += ", og kan lukke incident direkte i Dataverse";
                }
                if (_settings.EnableCarryForwardArealRun)
                {
                    runMessage += ", og kan køre et lokalt areal carry-forward-seam";
                }
                if (_settings.EnableArealSumQueueRun)
                {
                    runMessage += ", inkl. AREALSUM2KONTO-job";
                }
                if (_settings.TilladSendTilDigitalPost && _settings.EnableDigitalPostStubRun)
                {
                    runMessage += ", og kan stage digital post som note";
                }
                runMessage += ". GI-ækvivalent areal-closeout er fortsat ikke porteret.";
                _logger.Warning(runMessage);
            }
            else
            {
                _logger.Warning("RUN-mode uden AllowPartialRun er fortsat blokeret. Den endelige areal-closeout-vej er endnu ikke porteret.");
            }

            return _workflow.Execute(request);
        }
    }
}
