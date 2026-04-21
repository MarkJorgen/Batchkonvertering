using dk.gi.app.contact.registrering.optaelling.Application.Abstractions;
using dk.gi.app.contact.registrering.optaelling.Application.Services;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Config;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Crm;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Notifications;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Runtime;

namespace dk.gi.app.contact.registrering.optaelling.Infrastructure.Composition
{
    public sealed class ServiceRegistryResult
    {
        public Gi.Batch.Shared.Runtime.IJobOrchestrator Orchestrator { get; set; }
    }

    public static class ServiceRegistry
    {
        public static ServiceRegistryResult Build(string[] args)
        {
            var rawConfiguration = ContactRegistreringOptaellingConfigurationFactory.CreateRaw(args);
            var settings = ContactRegistreringOptaellingConfigurationFactory.CreateSettings(rawConfiguration);
            var startupDiagnostics = ContactRegistreringStartupDiagnostics.Build(rawConfiguration, settings);
            ContactRegistreringStartupDiagnostics.WriteToConsole(startupDiagnostics);
            ContactRegistreringOptaellingSettingsValidator.Validate(settings);
            var logger = JobLoggerFactory.Create(settings);
            var request = ContactRegistreringOptaellingRequestFactory.Create(rawConfiguration, settings);

            IFailureNotifier notifier = new ConfigurableFailureNotifier(rawConfiguration, settings.FailureRecipients);
            IContactRegistreringWorkflowClientFactory workflowClientFactory = new ContactRegistreringDataverseClientFactory(settings, logger);
            IContactRegistreringWorkflow workflow = new ContactRegistreringDataverseWorkflow(workflowClientFactory, logger);

            var guard = new SingleInstanceGuard(settings);
            var gateway = new ContactRegistreringOptaellingCrmGateway(settings, workflow, logger);
            var crmConnectionVerifier = new ContactRegistreringCrmConnectionVerifier(workflowClientFactory, logger);
            var failureNotificationService = new FailureNotificationService(settings, notifier);

            var orchestrator = new ContactRegistreringOptaellingOrchestrator(
                settings,
                request,
                guard,
                gateway,
                crmConnectionVerifier,
                failureNotificationService,
                logger);

            return new ServiceRegistryResult
            {
                Orchestrator = orchestrator
            };
        }
    }
}
