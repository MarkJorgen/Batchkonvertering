using System;
using System.Collections.Generic;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Messaging;
using Gi.Batch.Shared.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Tests.Unit
{
    [TestClass]
    public class RunoutRegistreringServiceBusSenderTests
    {
        [TestMethod]
        public void Send_Returns_False_When_Message_Settings_Are_Missing()
        {
            var settings = ContactRegistreringUdloebneOptaellingSettingsBuilder.Build(
                "RUN",
                serviceBusQueueName: "",
                serviceBusLabel: "",
                serviceBusSessionId: "");
            var logger = new CapturingJobLogger();
            var sender = new RunoutRegistreringServiceBusSender(settings, logger);

            bool result = sender.Send(
                new RunoutRegistreringCandidate(Guid.NewGuid(), "SAG-1", Guid.NewGuid()),
                new ResolvedServiceBusSettings("https://namespace.servicebus.windows.net", "RootManageSharedAccessKey", "secret", "crm config_configurationsetting"));

            Assert.IsFalse(result);
            StringAssert.Contains(logger.LastError ?? string.Empty, "ServiceBusQueueName");
            StringAssert.Contains(logger.LastError ?? string.Empty, "ServiceBusLabel");
            StringAssert.Contains(logger.LastError ?? string.Empty, "ServiceBusSessionId");
        }

        private sealed class CapturingJobLogger : IJobLogger
        {
            public string LastError { get; private set; }
            public void Info(string message) { }
            public void Warning(string message) { }
            public void Error(string message, Exception exception = null) { LastError = message; }
        }
    }
}
