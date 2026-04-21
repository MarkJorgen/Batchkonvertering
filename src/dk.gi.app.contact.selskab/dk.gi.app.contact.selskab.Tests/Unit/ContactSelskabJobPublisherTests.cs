using System;
using dk.gi.app.contact.selskab.Application.Models;
using dk.gi.app.contact.selskab.Infrastructure.Messaging;
using Gi.Batch.Shared.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.selskab.Tests.Unit
{
    [TestClass]
    public class ContactSelskabJobPublisherTests
    {
        [TestMethod]
        public void Publish_No_Candidates_Returns_Success_Without_Sends()
        {
            var settings = ContactSelskabSettings.Create(new Gi.Batch.Shared.Configuration.JobConfiguration(
                new System.Collections.Generic.Dictionary<string, string>
                {
                    ["Mode"] = "RUN",
                    ["CrmConnectionTemplate"] = "AuthType=ClientSecret;Url=https://{0};ClientId={1};ClientSecret={2};Authority=https://{3};",
                    ["CrmServerName"] = "server",
                    ["CrmClientId"] = "client",
                    ["CrmClientSecret"] = "secret",
                    ["CrmAuthority"] = "tenant"
                }));

            var sender = new FakeSender();
            var publisher = new ContactSelskabJobPublisher(settings, sender, new NullLogger());

            var result = publisher.Publish(Array.Empty<ContactSelskabCandidate>(), ResolvedServiceBusSettings.Empty("none"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.PublishedCount);
            Assert.AreEqual(0, sender.SendCount);
        }

        private sealed class FakeSender : ContactSelskabServiceBusSender
        {
            public int SendCount { get; private set; }

            public FakeSender()
                : base(
                    ContactSelskabSettings.Create(new Gi.Batch.Shared.Configuration.JobConfiguration(
                        new System.Collections.Generic.Dictionary<string, string>
                        {
                            ["Mode"] = "RUN",
                            ["CrmConnectionTemplate"] = "AuthType=ClientSecret;Url=https://{0};ClientId={1};ClientSecret={2};Authority=https://{3};",
                            ["CrmServerName"] = "server",
                            ["CrmClientId"] = "client",
                            ["CrmClientSecret"] = "secret",
                            ["CrmAuthority"] = "tenant"
                        })),
                    new NullLogger())
            {
            }

            public override bool Send(ContactSelskabCandidate candidate, ResolvedServiceBusSettings resolvedServiceBusSettings, int scheduleDelaySeconds)
            {
                SendCount++;
                return true;
            }
        }

        private sealed class NullLogger : IJobLogger
        {
            public void Info(string message) { }
            public void Warning(string message) { }
            public void Error(string message, Exception exception = null) { }
        }
    }
}
