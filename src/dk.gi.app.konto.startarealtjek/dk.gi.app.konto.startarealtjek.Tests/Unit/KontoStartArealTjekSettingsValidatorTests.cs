using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dk.gi.app.konto.startarealtjek.Application.Models;
using dk.gi.app.konto.startarealtjek.Infrastructure.Config;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.konto.startarealtjek.Tests.Unit
{
    [TestClass]
    public class KontoStartArealTjekSettingsValidatorTests
    {
        [TestMethod]
        public void Validate_Missing_ServiceBusQueueName_Throws()
        {
            var configuration = new JobConfiguration(new System.Collections.Generic.Dictionary<string, string>
            {
                ["Mode"] = "RUN",
                ["CrmConnectionTemplate"] = "AuthType=ClientSecret;Url=https://{0};ClientId={1};ClientSecret={2};Authority=https://{3};",
                ["CrmServerName"] = "server",
                ["CrmClientId"] = "client",
                ["CrmClientSecret"] = "secret",
                ["CrmAuthority"] = "tenant",
                ["ServiceBusQueueName"] = "",
                ["ServiceBusLabel"] = "ArealTjekKonto"
            });

            var settings = KontoStartArealTjekSettings.Create(configuration);

            Assert.ThrowsExactly<ConfigurationErrorsException>(() => KontoStartArealTjekSettingsValidator.Validate(settings));
        }
    }
}
