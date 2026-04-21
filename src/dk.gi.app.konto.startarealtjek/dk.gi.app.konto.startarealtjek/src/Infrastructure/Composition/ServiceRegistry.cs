using dk.gi.app.konto.startarealtjek.Application.Abstractions;
using dk.gi.app.konto.startarealtjek.Application.Services;
using dk.gi.app.konto.startarealtjek.Infrastructure.Config;
using dk.gi.app.konto.startarealtjek.Infrastructure.Crm;
using dk.gi.app.konto.startarealtjek.Infrastructure.Messaging;
using dk.gi.app.konto.startarealtjek.Infrastructure.Notifications;
using dk.gi.app.konto.startarealtjek.Infrastructure.Runtime;

namespace dk.gi.app.konto.startarealtjek.Infrastructure.Composition
{
    public sealed class ServiceRegistryResult
    {
        public Gi.Batch.Shared.Runtime.IJobOrchestrator Orchestrator { get; set; }
    }

    public static class ServiceRegistry
    {
        public static ServiceRegistryResult Build(string[] args)
        {
            var rawConfiguration = KontoStartArealTjekConfigurationFactory.CreateRaw(args);
            var settings = KontoStartArealTjekConfigurationFactory.CreateSettings(rawConfiguration);
            var startupDiagnostics = KontoStartArealTjekStartupDiagnostics.Build(rawConfiguration, settings);
            KontoStartArealTjekStartupDiagnostics.WriteToConsole(startupDiagnostics);
            KontoStartArealTjekSettingsValidator.Validate(settings);

            var logger = JobLoggerFactory.Create(settings);
            var request = KontoStartArealTjekRequestFactory.Create(rawConfiguration, settings);
            var selectionEngine = new KontoStartArealTjekSelectionEngine();

            IFailureNotifier notifier = new ConfigurableFailureNotifier(rawConfiguration, settings.FailureRecipients);
            IKontoStartArealTjekScanClientFactory scanClientFactory = new KontoStartArealTjekDataverseClientFactory(settings, logger);
            IKontoStartArealTjekJobPublisher jobPublisher = new KontoStartArealTjekJobPublisher(settings, new KontoStartArealTjekServiceBusSender(settings, logger), logger);
            IKontoStartArealTjekWorkflow workflow = new KontoStartArealTjekDataverseWorkflow(settings, scanClientFactory, selectionEngine, jobPublisher, logger);
            IKontoStartArealTjekGateway gateway = new KontoStartArealTjekCrmGateway(settings, workflow, logger);
            IKontoStartArealTjekCrmConnectionVerifier crmConnectionVerifier = new KontoStartArealTjekCrmConnectionVerifier(scanClientFactory, logger);

            var guard = new SingleInstanceGuard(settings);
            var failureNotificationService = new FailureNotificationService(settings, notifier);
            var orchestrator = new KontoStartArealTjekOrchestrator(settings, request, guard, gateway, crmConnectionVerifier, failureNotificationService, logger);
            return new ServiceRegistryResult { Orchestrator = orchestrator };
        }
    }
}
