using Microsoft.VisualStudio.TestTools.UnitTesting;
using dk.gi.app.contact.lassox.ophoer.Application.Abstractions;
using dk.gi.app.contact.lassox.ophoer.Application.Models;
using dk.gi.app.contact.lassox.ophoer.Infrastructure.Crm;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.lassox.ophoer.Tests.Unit
{
    [TestClass]
    public class LassoXOphoerCrmGatewayTests
    {
        [TestMethod]
        public void Execute_DryRun_KalderWorkflowIModeUdenWrites()
        {
            var workflow = new StubWorkflow();
            var settings = LassoXOphoerSettings.Create(new Gi.Batch.Shared.Configuration.JobConfiguration(new System.Collections.Generic.Dictionary<string, string>
            {
                ["Mode"] = "DRYRUN",
                ["CrmConnectionTemplate"] = "AuthType=ClientSecret;Url=https://{0};ClientId={1};ClientSecret={2};Authority=https://{3};",
                ["CrmServerName"] = "server",
                ["CrmClientId"] = "client",
                ["CrmClientSecret"] = "secret",
                ["CrmAuthority"] = "tenant"
            }));

            var gateway = new LassoXOphoerCrmGateway(settings, workflow, new NullJobLogger());
            var summary = gateway.Execute(new LassoXOphoerRequest(null, "DRYRUN", false));

            Assert.IsTrue(summary.Success);
            Assert.AreEqual(1, workflow.CallCount);
        }

        [TestMethod]
        public void Execute_Run_KalderWorkflow()
        {
            var workflow = new StubWorkflow();
            var settings = LassoXOphoerSettings.Create(new Gi.Batch.Shared.Configuration.JobConfiguration(new System.Collections.Generic.Dictionary<string, string>
            {
                ["Mode"] = "RUN",
                ["CrmConnectionTemplate"] = "AuthType=ClientSecret;Url=https://{0};ClientId={1};ClientSecret={2};Authority=https://{3};",
                ["CrmServerName"] = "server",
                ["CrmClientId"] = "client",
                ["CrmClientSecret"] = "secret",
                ["CrmAuthority"] = "tenant"
            }));

            var gateway = new LassoXOphoerCrmGateway(settings, workflow, new NullJobLogger());
            var summary = gateway.Execute(new LassoXOphoerRequest(null, "RUN", true));

            Assert.IsTrue(summary.Success);
            Assert.AreEqual(1, workflow.CallCount);
        }

        private sealed class StubWorkflow : ILassoXOphoerWorkflow
        {
            public int CallCount { get; private set; }

            public LassoXOphoerExecutionSummary Execute(LassoXOphoerRequest request)
            {
                CallCount++;
                return new LassoXOphoerExecutionSummary(true, 1, 1, request.WriteChanges ? 1 : 0, "ok", "stub");
            }
        }
    }
}
