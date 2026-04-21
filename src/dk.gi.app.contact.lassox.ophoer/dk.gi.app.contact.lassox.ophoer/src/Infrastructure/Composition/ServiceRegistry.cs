using dk.gi.app.contact.lassox.ophoer.Application.Abstractions;
using dk.gi.app.contact.lassox.ophoer.Application.Services;
using dk.gi.app.contact.lassox.ophoer.Infrastructure.Config;
using dk.gi.app.contact.lassox.ophoer.Infrastructure.Crm;
using dk.gi.app.contact.lassox.ophoer.Infrastructure.Notifications;
using dk.gi.app.contact.lassox.ophoer.Infrastructure.Runtime;

namespace dk.gi.app.contact.lassox.ophoer.Infrastructure.Composition
{
    public sealed class ServiceRegistryResult
    {
        public Gi.Batch.Shared.Runtime.IJobOrchestrator Orchestrator { get; set; }
    }

    public static class ServiceRegistry
    {
        public static ServiceRegistryResult Build(string[] args)
        {
            var rawConfiguration = LassoXOphoerConfigurationFactory.CreateRaw(args);
            var settings = LassoXOphoerConfigurationFactory.CreateSettings(rawConfiguration);
            var startupDiagnostics = LassoXOphoerStartupDiagnostics.Build(rawConfiguration, settings);
            LassoXOphoerStartupDiagnostics.WriteToConsole(startupDiagnostics);
            LassoXOphoerSettingsValidator.Validate(settings);

            var logger = JobLoggerFactory.Create(settings);
            var request = LassoXOphoerRequestFactory.Create(rawConfiguration, settings);
            var decisionEngine = new LassoXOphoerDecisionEngine();

            IFailureNotifier notifier = new ConfigurableFailureNotifier(rawConfiguration, settings.FailureRecipients);
            ILassoXOphoerScanClientFactory scanClientFactory = new LassoXOphoerDataverseClientFactory(settings, logger);
            ILassoXOphoerWorkflow workflow = new LassoXOphoerDataverseWorkflow(scanClientFactory, decisionEngine, logger);
            ILassoXOphoerGateway gateway = new LassoXOphoerCrmGateway(settings, workflow, logger);
            ILassoXOphoerCrmConnectionVerifier crmConnectionVerifier = new LassoXOphoerCrmConnectionVerifier(scanClientFactory, logger);

            var guard = new SingleInstanceGuard(settings);
            var failureNotificationService = new FailureNotificationService(settings, notifier);
            var orchestrator = new LassoXOphoerOrchestrator(settings, request, guard, gateway, crmConnectionVerifier, failureNotificationService, logger);
            return new ServiceRegistryResult { Orchestrator = orchestrator };
        }
    }
}
