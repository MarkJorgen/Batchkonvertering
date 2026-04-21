using System;
using dk.gi.app.contact.registrering.optaelling.Application.Abstractions;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Crm;
using Gi.Batch.Shared.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registrering.optaelling.Tests.Unit
{
    [TestClass]
    public class ContactRegistreringDataverseWorkflowTests
    {
        [TestMethod]
        public void Execute_Returns_Failure_When_CloseExpired_Fails()
        {
            var workflow = new ContactRegistreringDataverseWorkflow(
                new StubFactory(new StubClient(
                    closeExpired: new ContactRegistreringExecutionSummary(false, false, false, "close failed", "client"),
                    createJobs: new ContactRegistreringExecutionSummary(true, false, false, "created", "client"))),
                new NullJobLogger());

            var result = workflow.Execute(Guid.Empty);

            Assert.IsFalse(result.Success);
            StringAssert.Contains(result.Message, "close failed");
        }

        [TestMethod]
        public void Execute_Returns_Success_When_Both_Operations_Succeed()
        {
            var workflow = new ContactRegistreringDataverseWorkflow(
                new StubFactory(new StubClient(
                    closeExpired: new ContactRegistreringExecutionSummary(true, false, false, "closed", "client"),
                    createJobs: new ContactRegistreringExecutionSummary(true, false, false, "created", "client"))),
                new NullJobLogger());

            var result = workflow.Execute(Guid.Empty);

            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.ClosedExpiredTreklipOwnerRegistrations);
            Assert.IsTrue(result.CreatedJobsForContacts);
        }

        private sealed class StubFactory : IContactRegistreringWorkflowClientFactory
        {
            private readonly IContactRegistreringWorkflowClient _client;

            public StubFactory(IContactRegistreringWorkflowClient client)
            {
                _client = client;
            }

            public IContactRegistreringWorkflowClient Create()
            {
                return _client;
            }
        }

        private sealed class StubClient : IContactRegistreringWorkflowClient
        {
            private readonly ContactRegistreringExecutionSummary _closeExpired;
            private readonly ContactRegistreringExecutionSummary _createJobs;

            public StubClient(ContactRegistreringExecutionSummary closeExpired, ContactRegistreringExecutionSummary createJobs)
            {
                _closeExpired = closeExpired;
                _createJobs = createJobs;
            }

            public ContactRegistreringExecutionSummary VerifyConnection() => new ContactRegistreringExecutionSummary(true, false, false, "verified", "client");
            public ContactRegistreringExecutionSummary CloseExpiredTreklipOwnerRegistrations() => _closeExpired;
            public ContactRegistreringExecutionSummary CreateJobsForContactRegistrerings(Guid? registreringId) => _createJobs;
            public void Dispose() { }
        }
    }
}
