using System;
using dk.gi.app.contact.registrering.optaelling.Application.Abstractions;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using dk.gi.app.contact.registrering.optaelling.Application.Services;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Notifications;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Runtime;
using Gi.Batch.Shared.Execution;
using Gi.Batch.Shared.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registrering.optaelling.Tests.Unit
{
    [TestClass]
    public class ContactRegistreringOptaellingOrchestratorTests
    {
        [TestMethod]
        public void Run_Returns_Ok_In_DryRun_Mode()
        {
            var settings = ContactRegistreringOptaellingSettingsBuilder.Build(mode: "DRYRUN");
            var gateway = new StubGateway(new ContactRegistreringExecutionSummary(true, true, true, "ok", "gateway"));
            var verifier = new StubVerifier(new ContactRegistreringExecutionSummary(true, false, false, "verified", "verifier"));
            var orchestrator = CreateOrchestrator(settings, gateway, verifier);

            JobExecutionResult result = orchestrator.Run();

            Assert.AreEqual(0, result.ExitCode);
            Assert.AreEqual(1, gateway.CallCount);
            Assert.AreEqual(0, verifier.CallCount);
        }

        [TestMethod]
        public void Run_Uses_VerifyCrmOnly_Mode_When_Configured()
        {
            var settings = ContactRegistreringOptaellingSettingsBuilder.Build(mode: "VERIFYCRM");
            var gateway = new StubGateway(new ContactRegistreringExecutionSummary(true, true, true, "ok", "gateway"));
            var verifier = new StubVerifier(new ContactRegistreringExecutionSummary(true, false, false, "verified", "verifier"));
            var orchestrator = CreateOrchestrator(settings, gateway, verifier);

            JobExecutionResult result = orchestrator.Run();

            Assert.AreEqual(0, result.ExitCode);
            Assert.AreEqual(0, gateway.CallCount);
            Assert.AreEqual(1, verifier.CallCount);
            StringAssert.Contains(result.Message, "VERIFYCRM gennemført");
        }

        private static ContactRegistreringOptaellingOrchestrator CreateOrchestrator(
            ContactRegistreringOptaellingSettings settings,
            IContactRegistreringOptaellingGateway gateway,
            IContactRegistreringCrmConnectionVerifier verifier)
        {
            var request = new ContactRegistreringOptaellingRequest(null, settings.Mode);
            var guard = new SingleInstanceGuard(settings);
            var notifier = new FailureNotificationService(settings, new ConsoleFailureNotifier());
            var logger = new NullJobLogger();

            return new ContactRegistreringOptaellingOrchestrator(settings, request, guard, gateway, verifier, notifier, logger);
        }

        private sealed class StubGateway : IContactRegistreringOptaellingGateway
        {
            private readonly ContactRegistreringExecutionSummary _summary;

            public StubGateway(ContactRegistreringExecutionSummary summary)
            {
                _summary = summary;
            }

            public int CallCount { get; private set; }

            public ContactRegistreringExecutionSummary Execute(ContactRegistreringOptaellingRequest request)
            {
                CallCount++;
                return _summary;
            }
        }

        private sealed class StubVerifier : IContactRegistreringCrmConnectionVerifier
        {
            private readonly ContactRegistreringExecutionSummary _summary;

            public StubVerifier(ContactRegistreringExecutionSummary summary)
            {
                _summary = summary;
            }

            public int CallCount { get; private set; }

            public ContactRegistreringExecutionSummary Verify()
            {
                CallCount++;
                return _summary;
            }
        }
    }
}
