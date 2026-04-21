using dk.gi.app.konto.afslutarealsager.Application.Abstractions;
using dk.gi.app.konto.afslutarealsager.Application.Services;
using dk.gi.app.konto.afslutarealsager.Infrastructure.Config;
using dk.gi.app.konto.afslutarealsager.Infrastructure.Crm;
using dk.gi.app.konto.afslutarealsager.Infrastructure.Crm.Dataverse;
using dk.gi.app.konto.afslutarealsager.Infrastructure.Documents;
using dk.gi.app.konto.afslutarealsager.Infrastructure.Messaging;
using dk.gi.app.konto.afslutarealsager.Infrastructure.Notifications;
using dk.gi.app.konto.afslutarealsager.Infrastructure.Runtime;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Composition
{
    public sealed class ServiceRegistryResult
    {
        public Gi.Batch.Shared.Runtime.IJobOrchestrator Orchestrator { get; set; }
    }

    public static class ServiceRegistry
    {
        public static ServiceRegistryResult Build(string[] args)
        {
            var rawConfiguration = KontoAfslutArealSagerConfigurationFactory.CreateRaw(args);
            var settings = KontoAfslutArealSagerConfigurationFactory.CreateSettings(rawConfiguration);
            var startupDiagnostics = KontoAfslutArealSagerStartupDiagnostics.Build(rawConfiguration, settings);
            KontoAfslutArealSagerStartupDiagnostics.WriteToConsole(startupDiagnostics);
            KontoAfslutArealSagerSettingsValidator.Validate(settings);

            var logger = JobLoggerFactory.Create(settings);
            var request = KontoAfslutArealSagerRequestFactory.Create(rawConfiguration, settings);
            IFailureNotifier notifier = new ConfigurableFailureNotifier(rawConfiguration, settings.FailureRecipients);
            IKontoAfslutArealSagerScanClientFactory scanClientFactory = new KontoAfslutArealSagerDataverseClientFactory(settings, logger);
            ILetterGenerator letterGenerator = new ArealLukLetterGenerator();
            IKontoAfslutArealSagerCloseoutPublisher closeoutPublisher = new KontoAfslutArealSagerCloseoutPublisher(new KontoAfslutArealSagerCloseoutServiceBusSender(settings, logger));
            IKontoAfslutArealSagerArealSumPublisher arealSumPublisher = new KontoAfslutArealSagerArealSumPublisher(new KontoAfslutArealSagerArealSumServiceBusSender(settings, logger));
            IKontoAfslutArealSagerWorkflow workflow = new KontoAfslutArealSagerDataverseWorkflow(settings, scanClientFactory, letterGenerator, closeoutPublisher, arealSumPublisher, logger);
            IKontoAfslutArealSagerGateway gateway = new KontoAfslutArealSagerCrmGateway(settings, workflow, logger);
            IKontoAfslutArealSagerCrmConnectionVerifier crmConnectionVerifier = new KontoAfslutArealSagerCrmConnectionVerifier(scanClientFactory, logger);

            var guard = new SingleInstanceGuard(settings);
            var failureNotificationService = new FailureNotificationService(settings, notifier);
            var orchestrator = new KontoAfslutArealSagerOrchestrator(settings, request, guard, gateway, crmConnectionVerifier, failureNotificationService, logger);
            return new ServiceRegistryResult { Orchestrator = orchestrator };
        }
    }
}
