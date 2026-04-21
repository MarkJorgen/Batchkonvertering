using System;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Messaging;
using Gi.Batch.Shared.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Tests.Unit
{
    [TestClass]
    public class RunoutRegistreringJobPublisherTests
    {
        [TestMethod]
        public void Publish_Keeps_Legacy_Behaviour_When_No_Closed_Treklip_Exist()
        {
            var settings = ContactRegistreringUdloebneOptaellingSettingsBuilder.Build(
                "RUN",
                serviceBusQueueName: "crmpluginjobs",
                serviceBusLabel: "laan",
                serviceBusSessionId: "842DAF62-91EC-4E25-9DE8-C109FBA3DAD1");
            var sender = new StubSender(settings, new NullJobLogger(), true);
            var publisher = new RunoutRegistreringJobPublisher(settings, sender, new NullJobLogger());

            var result = publisher.Publish(
                new[] { new RunoutRegistreringCandidate(Guid.NewGuid(), "SAG-1", Guid.NewGuid()) },
                new Guid[0],
                ResolvedServiceBusSettings.Empty("test"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.PublishedCount);
            Assert.AreEqual(1, result.SkippedCount);
        }

        [TestMethod]
        public void Publish_Fails_When_Candidate_Must_Be_Sent_And_Sender_Fails()
        {
            var settings = ContactRegistreringUdloebneOptaellingSettingsBuilder.Build(
                "RUN",
                serviceBusQueueName: "crmpluginjobs",
                serviceBusLabel: "laan",
                serviceBusSessionId: "842DAF62-91EC-4E25-9DE8-C109FBA3DAD1");
            var sender = new StubSender(settings, new NullJobLogger(), false);
            var publisher = new RunoutRegistreringJobPublisher(settings, sender, new NullJobLogger());
            Guid treklipId = Guid.NewGuid();

            var result = publisher.Publish(
                new[] { new RunoutRegistreringCandidate(Guid.NewGuid(), "SAG-1", treklipId) },
                new[] { treklipId },
                new ResolvedServiceBusSettings("https://namespace.servicebus.windows.net", "RootManageSharedAccessKey", "secret", "test"));

            Assert.IsFalse(result.Success);
            StringAssert.Contains(result.Message, "Service Bus");
        }

        private sealed class StubSender : RunoutRegistreringServiceBusSender
        {
            private readonly bool _result;

            public StubSender(ContactRegistreringUdloebneOptaellingSettings settings, IJobLogger logger, bool result) : base(settings, logger)
            {
                _result = result;
            }

            public override bool Send(RunoutRegistreringCandidate candidate, ResolvedServiceBusSettings resolvedServiceBusSettings) => _result;
        }
    }
}
