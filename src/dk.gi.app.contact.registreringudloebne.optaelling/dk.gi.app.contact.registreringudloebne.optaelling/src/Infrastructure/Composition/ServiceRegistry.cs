using dk.gi.app.contact.registreringudloebne.optaelling.Application.Abstractions;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Services;
using dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Config;
using dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Crm;
using dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Messaging;
using dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Notifications;
using dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Runtime;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Composition
{
    public sealed class ServiceRegistryResult
    {
        public Gi.Batch.Shared.Runtime.IJobOrchestrator Orchestrator { get; set; }
    }

    public static class ServiceRegistry
    {
        public static ServiceRegistryResult Build(string[] args)
        {
            var rawConfiguration = ContactRegistreringUdloebneOptaellingConfigurationFactory.CreateRaw(args);
            var settings = ContactRegistreringUdloebneOptaellingConfigurationFactory.CreateSettings(rawConfiguration);
            var startupDiagnostics = ContactRegistreringUdloebneStartupDiagnostics.Build(rawConfiguration, settings);
            ContactRegistreringUdloebneStartupDiagnostics.WriteToConsole(startupDiagnostics);
            ContactRegistreringUdloebneOptaellingSettingsValidator.Validate(settings);

            var logger = JobLoggerFactory.Create(settings);
            var request = ContactRegistreringUdloebneOptaellingRequestFactory.Create(rawConfiguration, settings);

            IFailureNotifier notifier = new ConfigurableFailureNotifier(rawConfiguration, settings.FailureRecipients);
            IRunoutRegistreringScanClientFactory scanClientFactory = new ContactRegistreringUdloebneDataverseClientFactory(settings, logger);
            IRunoutRegistreringJobPublisher jobPublisher = new RunoutRegistreringJobPublisher(settings, new RunoutRegistreringServiceBusSender(settings, logger), logger);
            IContactRegistreringUdloebneWorkflow workflow = new ContactRegistreringUdloebneDataverseWorkflow(scanClientFactory, jobPublisher, logger);

            var guard = new SingleInstanceGuard(settings);
            var gateway = new ContactRegistreringUdloebneOptaellingCrmGateway(settings, workflow, logger);
            var crmConnectionVerifier = new ContactRegistreringUdloebneCrmConnectionVerifier(scanClientFactory, logger);
            var failureNotificationService = new FailureNotificationService(settings, notifier);

            var orchestrator = new ContactRegistreringUdloebneOptaellingOrchestrator(settings, request, guard, gateway, crmConnectionVerifier, failureNotificationService, logger);
            return new ServiceRegistryResult { Orchestrator = orchestrator };
        }
    }
}
