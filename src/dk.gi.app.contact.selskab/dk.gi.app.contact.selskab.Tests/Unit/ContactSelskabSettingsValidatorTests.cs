using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dk.gi.app.contact.selskab.Application.Models;
using dk.gi.app.contact.selskab.Infrastructure.Config;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.contact.selskab.Tests.Unit
{
    [TestClass]
    public class ContactSelskabSettingsValidatorTests
    {
        [TestMethod]
        public void Validate_RunMode_Missing_ServiceBusQueueName_Throws()
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
                ["ServiceBusLabel"] = "KontoDiv"
            });

            var settings = ContactSelskabSettings.Create(configuration);

            Assert.ThrowsExactly<ConfigurationErrorsException>(() => ContactSelskabSettingsValidator.Validate(settings));
        }
    }
}
