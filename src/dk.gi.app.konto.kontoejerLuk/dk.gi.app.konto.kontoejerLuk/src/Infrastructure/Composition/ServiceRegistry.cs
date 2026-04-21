using dk.gi.app.konto.kontoejerLuk.Application.Abstractions;
using dk.gi.app.konto.kontoejerLuk.Application.Services;
using dk.gi.app.konto.kontoejerLuk.Infrastructure.Config;
using dk.gi.app.konto.kontoejerLuk.Infrastructure.Crm;
using dk.gi.app.konto.kontoejerLuk.Infrastructure.Crm.Dataverse;
using dk.gi.app.konto.kontoejerLuk.Infrastructure.Notifications;
using dk.gi.app.konto.kontoejerLuk.Infrastructure.Runtime;

namespace dk.gi.app.konto.kontoejerLuk.Infrastructure.Composition
{
    public sealed class ServiceRegistryResult
    {
        public Gi.Batch.Shared.Runtime.IJobOrchestrator Orchestrator { get; set; }
    }

    public static class ServiceRegistry
    {
        public static ServiceRegistryResult Build(string[] args)
        {
            var rawConfiguration = KontoejerLukConfigurationFactory.CreateRaw(args);
            var settings = KontoejerLukConfigurationFactory.CreateSettings(rawConfiguration);
            var startupDiagnostics = KontoejerLukStartupDiagnostics.Build(rawConfiguration, settings);
            KontoejerLukStartupDiagnostics.WriteToConsole(startupDiagnostics);
            KontoejerLukSettingsValidator.Validate(settings);

            var logger = JobLoggerFactory.Create(settings);
            var request = KontoejerLukRequestFactory.Create(rawConfiguration, settings);
            var planner = new KontoejerLukPlanner();

            IFailureNotifier notifier = new ConfigurableFailureNotifier(rawConfiguration, settings.FailureRecipients);
            IKontoejerLukScanClientFactory scanClientFactory = new KontoejerLukDataverseClientFactory(settings, logger);
            IKontoejerLukWorkflow workflow = new KontoejerLukDataverseWorkflow(settings, scanClientFactory, planner, logger);
            IKontoejerLukGateway gateway = new KontoejerLukCrmGateway(settings, workflow, logger);
            IKontoejerLukCrmConnectionVerifier crmConnectionVerifier = new KontoejerLukCrmConnectionVerifier(scanClientFactory, logger);

            var guard = new SingleInstanceGuard(settings);
            var failureNotificationService = new FailureNotificationService(settings, notifier);
            var orchestrator = new KontoejerLukOrchestrator(settings, request, guard, gateway, crmConnectionVerifier, failureNotificationService, logger);
            return new ServiceRegistryResult { Orchestrator = orchestrator };
        }
    }
}
