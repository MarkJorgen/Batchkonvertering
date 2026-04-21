using dk.gi.app.contact.selskab.Application.Abstractions;
using dk.gi.app.contact.selskab.Application.Services;
using dk.gi.app.contact.selskab.Infrastructure.Config;
using dk.gi.app.contact.selskab.Infrastructure.Crm;
using dk.gi.app.contact.selskab.Infrastructure.Messaging;
using dk.gi.app.contact.selskab.Infrastructure.Notifications;
using dk.gi.app.contact.selskab.Infrastructure.Runtime;

namespace dk.gi.app.contact.selskab.Infrastructure.Composition
{
    public sealed class ServiceRegistryResult
    {
        public Gi.Batch.Shared.Runtime.IJobOrchestrator Orchestrator { get; set; }
    }

    public static class ServiceRegistry
    {
        public static ServiceRegistryResult Build(string[] args)
        {
            var rawConfiguration = ContactSelskabConfigurationFactory.CreateRaw(args);
            var settings = ContactSelskabConfigurationFactory.CreateSettings(rawConfiguration);
            var startupDiagnostics = ContactSelskabStartupDiagnostics.Build(rawConfiguration, settings);
            ContactSelskabStartupDiagnostics.WriteToConsole(startupDiagnostics);
            ContactSelskabSettingsValidator.Validate(settings);

            var logger = JobLoggerFactory.Create(settings);
            var request = ContactSelskabRequestFactory.Create(rawConfiguration, settings);
            var selectionEngine = new ContactSelskabSelectionEngine();

            IFailureNotifier notifier = new ConfigurableFailureNotifier(rawConfiguration, settings.FailureRecipients);
            IContactSelskabScanClientFactory scanClientFactory = new ContactSelskabDataverseClientFactory(settings, logger);
            IContactSelskabJobPublisher jobPublisher = new ContactSelskabJobPublisher(settings, new ContactSelskabServiceBusSender(settings, logger), logger);
            IContactSelskabWorkflow workflow = new ContactSelskabDataverseWorkflow(settings, scanClientFactory, selectionEngine, jobPublisher, logger);
            IContactSelskabGateway gateway = new ContactSelskabCrmGateway(settings, workflow, logger);
            IContactSelskabCrmConnectionVerifier crmConnectionVerifier = new ContactSelskabCrmConnectionVerifier(scanClientFactory, logger);

            var guard = new SingleInstanceGuard(settings);
            var failureNotificationService = new FailureNotificationService(settings, notifier);
            var orchestrator = new ContactSelskabOrchestrator(settings, request, guard, gateway, crmConnectionVerifier, failureNotificationService, logger);
            return new ServiceRegistryResult { Orchestrator = orchestrator };
        }
    }
}
