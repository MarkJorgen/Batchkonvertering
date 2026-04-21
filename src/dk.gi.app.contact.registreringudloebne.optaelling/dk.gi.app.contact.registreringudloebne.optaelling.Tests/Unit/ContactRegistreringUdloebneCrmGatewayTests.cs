using System;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Abstractions;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Crm;
using Gi.Batch.Shared.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Tests.Unit
{
    [TestClass]
    public class ContactRegistreringUdloebneCrmGatewayTests
    {
        [TestMethod]
        public void Execute_Returns_DryRun_Summary_When_Mode_Is_DryRun()
        {
            var settings = ContactRegistreringUdloebneOptaellingSettingsBuilder.Build("DRYRUN");
            var gateway = new ContactRegistreringUdloebneOptaellingCrmGateway(settings, new StubWorkflow(), new NullJobLogger());
            var result = gateway.Execute(new ContactRegistreringUdloebneOptaellingRequest(null, settings.Mode));
            Assert.IsTrue(result.Success);
            StringAssert.Contains(result.Message, "DRYRUN");
        }

        private sealed class StubWorkflow : IContactRegistreringUdloebneWorkflow
        {
            public ContactRegistreringUdloebneExecutionSummary Execute(Guid? registreringId)
                => new ContactRegistreringUdloebneExecutionSummary(true, 1, 1, "ok", "stub");
        }
    }
}
