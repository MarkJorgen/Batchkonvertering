using System;
using dk.gi.app.konto.kontoejerLuk.Infrastructure.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.konto.kontoejerLuk.Tests.Smoke
{
    [TestClass]
    public class ServiceRegistrySmokeTests
    {
        [TestMethod]
        public void Build_ReturnsOrchestrator_WhenRequiredArgsProvided()
        {
            var result = ServiceRegistry.Build(new[]
            {
                "Mode=DRYRUN",
                "CrmConnectionTemplate=AuthType=ClientSecret;Url={0};ClientId={1};ClientSecret={2};Authority={3};",
                "CrmServerName=https://example.crm4.dynamics.com",
                "CrmClientId=client",
                "CrmClientSecret=secret",
                "CrmAuthority=https://login.microsoftonline.com/tenant"
            });

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Orchestrator);
        }
    }
}
