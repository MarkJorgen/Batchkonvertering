using System;
using dk.gi.app.contact.registrering.optaelling.Application.Abstractions;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Crm;
using Gi.Batch.Shared.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registrering.optaelling.Tests.Unit
{
    [TestClass]
    public class ContactRegistreringOptaellingCrmGatewayTests
    {
        [TestMethod]
        public void Execute_Returns_DryRun_Summary_When_Mode_Is_DryRun()
        {
            var settings = ContactRegistreringOptaellingSettingsBuilder.Build(mode: "DRYRUN");
            var gateway = new ContactRegistreringOptaellingCrmGateway(
                settings,
                new StubWorkflow(),
                new ConsoleJobLogger());

            var result = gateway.Execute(new ContactRegistreringOptaellingRequest(null, settings.Mode));

            Assert.IsTrue(result.Success);
            StringAssert.Contains(result.Message, "DRYRUN");
            Assert.AreEqual("DRYRUN mapper", result.Source);
        }

        [TestMethod]
        public void Execute_Uses_Workflow_When_Mode_Is_Run()
        {
            var settings = ContactRegistreringOptaellingSettingsBuilder.Build(mode: "RUN");
            var workflow = new StubWorkflow();
            var gateway = new ContactRegistreringOptaellingCrmGateway(
                settings,
                workflow,
                new ConsoleJobLogger());

            var result = gateway.Execute(new ContactRegistreringOptaellingRequest(Guid.Empty, settings.Mode));

            Assert.IsTrue(workflow.WasCalled);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("stub workflow", result.Source);
        }

        private sealed class StubWorkflow : IContactRegistreringWorkflow
        {
            public bool WasCalled { get; private set; }

            public ContactRegistreringExecutionSummary Execute(Guid? registreringId)
            {
                WasCalled = true;
                return new ContactRegistreringExecutionSummary(true, true, true, "ok", "stub workflow");
            }
        }
    }
}
