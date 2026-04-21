using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dk.gi.app.contact.lassox.ophoer.Infrastructure.Composition;

namespace dk.gi.app.contact.lassox.ophoer.Tests.Smoke
{
    [TestClass]
    public class ServiceRegistrySmokeTests
    {
        [TestMethod]
        public void Build_DryRun_UdenConfigStore_KanByggeRegistry()
        {
            IDictionary environment = System.Environment.GetEnvironmentVariables();
            string original = environment["AZURE_APPCONFIG_CONNECTIONSTRING"] as string;
            try
            {
                System.Environment.SetEnvironmentVariable("AZURE_APPCONFIG_CONNECTIONSTRING", null);
                var result = ServiceRegistry.Build(new[]
                {
                    "-UseConfigStore=false",
                    "-Mode=DRYRUN",
                    "-CrmConnectionTemplate=AuthType=ClientSecret;Url=https://{0};ClientId={1};ClientSecret={2};Authority=https://{3};",
                    "-CrmServerName=server",
                    "-CrmClientId=client",
                    "-CrmClientSecret=secret",
                    "-CrmAuthority=tenant"
                });
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Orchestrator);
            }
            finally
            {
                System.Environment.SetEnvironmentVariable("AZURE_APPCONFIG_CONNECTIONSTRING", original);
            }
        }
    }
}
