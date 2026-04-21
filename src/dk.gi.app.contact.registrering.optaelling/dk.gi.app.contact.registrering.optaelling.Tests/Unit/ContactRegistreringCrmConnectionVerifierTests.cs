using System;
using dk.gi.app.contact.registrering.optaelling.Application.Abstractions;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Crm;
using Gi.Batch.Shared.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registrering.optaelling.Tests.Unit
{
    [TestClass]
    public class ContactRegistreringCrmConnectionVerifierTests
    {
        [TestMethod]
        public void Verify_Returns_Failure_When_Factory_Throws()
        {
            var verifier = new ContactRegistreringCrmConnectionVerifier(new ThrowingFactory(), new NullJobLogger());

            var result = verifier.Verify();

            Assert.IsFalse(result.Success);
            StringAssert.Contains(result.Message, "VERIFYCRM fejlede");
        }

        [TestMethod]
        public void Verify_Returns_Success_When_Client_Verifies_Connection()
        {
            var verifier = new ContactRegistreringCrmConnectionVerifier(
                new StubFactory(new StubClient()),
                new NullJobLogger());

            var result = verifier.Verify();

            Assert.IsTrue(result.Success);
            StringAssert.Contains(result.Message, "CRM-forbindelse valideret");
        }

        private sealed class ThrowingFactory : IContactRegistreringWorkflowClientFactory
        {
            public IContactRegistreringWorkflowClient Create()
            {
                throw new InvalidOperationException("cannot connect");
            }
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
            public ContactRegistreringExecutionSummary VerifyConnection()
            {
                return new ContactRegistreringExecutionSummary(true, false, false, "CRM-forbindelse valideret. Der blev ikke udført workflow-kald i VERIFYCRM-mode.", "client");
            }

            public ContactRegistreringExecutionSummary CloseExpiredTreklipOwnerRegistrations()
            {
                throw new NotSupportedException();
            }

            public ContactRegistreringExecutionSummary CreateJobsForContactRegistrerings(Guid? registreringId)
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            {
            }
        }
    }
}
