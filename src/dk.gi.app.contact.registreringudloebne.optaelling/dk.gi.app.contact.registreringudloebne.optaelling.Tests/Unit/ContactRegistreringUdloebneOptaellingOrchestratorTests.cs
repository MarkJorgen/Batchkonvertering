using dk.gi.app.contact.registreringudloebne.optaelling.Application.Abstractions;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Services;
using dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Notifications;
using dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Runtime;
using Gi.Batch.Shared.Execution;
using Gi.Batch.Shared.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Tests.Unit
{
    [TestClass]
    public class ContactRegistreringUdloebneOptaellingOrchestratorTests
    {
        [TestMethod]
        public void Run_Uses_Verify_Mode_When_Configured()
        {
            var settings = ContactRegistreringUdloebneOptaellingSettingsBuilder.Build("VERIFYCRM");
            var orchestrator = new ContactRegistreringUdloebneOptaellingOrchestrator(
                settings,
                new ContactRegistreringUdloebneOptaellingRequest(null, settings.Mode),
                new SingleInstanceGuard(settings),
                new StubGateway(),
                new StubVerifier(),
                new FailureNotificationService(settings, new ConsoleFailureNotifier()),
                new NullJobLogger());

            JobExecutionResult result = orchestrator.Run();

            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains(result.Message, "VERIFYCRM gennemført");
        }

        private sealed class StubGateway : IContactRegistreringUdloebneOptaellingGateway
        {
            public ContactRegistreringUdloebneExecutionSummary Execute(ContactRegistreringUdloebneOptaellingRequest request)
                => new ContactRegistreringUdloebneExecutionSummary(true, 1, 1, "ok", "gateway");
        }

        private sealed class StubVerifier : IContactRegistreringUdloebneCrmConnectionVerifier
        {
            public ContactRegistreringUdloebneExecutionSummary Verify()
                => new ContactRegistreringUdloebneExecutionSummary(true, 0, 0, "verified", "verifier");
        }
    }
}
