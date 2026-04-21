using System;
using dk.gi.app.konto.startarealtjek.Application.Models;
using dk.gi.app.konto.startarealtjek.Infrastructure.Messaging;
using Gi.Batch.Shared.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.konto.startarealtjek.Tests.Unit
{
    [TestClass]
    public class KontoStartArealTjekJobPublisherTests
    {
        [TestMethod]
        public void Publish_No_Candidates_Returns_Success_Without_Sends()
        {
            var settings = KontoStartArealTjekSettings.Create(new Gi.Batch.Shared.Configuration.JobConfiguration(
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
            var publisher = new KontoStartArealTjekJobPublisher(settings, sender, new NullLogger());

            var result = publisher.Publish(Array.Empty<KontoStartArealTjekCandidate>(), ResolvedServiceBusSettings.Empty("none"));

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.PublishedCount);
            Assert.AreEqual(0, sender.SendCount);
        }

        private sealed class FakeSender : KontoStartArealTjekServiceBusSender
        {
            public int SendCount { get; private set; }

            public FakeSender()
                : base(
                    KontoStartArealTjekSettings.Create(new Gi.Batch.Shared.Configuration.JobConfiguration(
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

            public override bool Send(KontoStartArealTjekCandidate candidate, ResolvedServiceBusSettings resolvedServiceBusSettings, int scheduleDelaySeconds)
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
