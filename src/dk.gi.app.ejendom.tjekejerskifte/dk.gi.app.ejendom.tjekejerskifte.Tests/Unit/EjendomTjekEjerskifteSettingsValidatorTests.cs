using System.Collections.Generic;
using System.Configuration;
using dk.gi.app.ejendom.tjekejerskifte.Application.Models;
using dk.gi.app.ejendom.tjekejerskifte.Infrastructure.Config;
using Gi.Batch.Shared.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.ejendom.tjekejerskifte.Tests.Unit
{
    [TestClass]
    public class EjendomTjekEjerskifteSettingsValidatorTests
    {
        [TestMethod]
        public void Validate_DoesNotThrow_ForMinimumValidSettings()
        {
            var settings = EjendomTjekEjerskifteSettings.Create(new JobConfiguration(new Dictionary<string, string>
            {
                ["Mode"] = "DRYRUN",
                ["MaxDage"] = "30",
                ["MaxAntal"] = "100",
                ["CrmConnectionTemplate"] = "AuthType=ClientSecret;Url={0};ClientId={1};ClientSecret={2};Authority={3};",
                ["CrmServerName"] = "https://example.crm4.dynamics.com",
                ["CrmClientId"] = "client",
                ["CrmClientSecret"] = "secret",
                ["CrmAuthority"] = "authority",
                ["ServiceBusQueueName"] = "crmpluginjobs",
                ["ServiceBusLabel"] = "TINGLYSNINGDATO"
            }));

            EjendomTjekEjerskifteSettingsValidator.Validate(settings);
        }

        [TestMethod]
        public void Validate_Throws_WhenMaxAntalTooHigh()
        {
            var settings = EjendomTjekEjerskifteSettings.Create(new JobConfiguration(new Dictionary<string, string>
            {
                ["Mode"] = "DRYRUN",
                ["MaxDage"] = "30",
                ["MaxAntal"] = "5000",
                ["CrmConnectionTemplate"] = "AuthType=ClientSecret;Url={0};ClientId={1};ClientSecret={2};Authority={3};",
                ["CrmServerName"] = "https://example.crm4.dynamics.com",
                ["CrmClientId"] = "client",
                ["CrmClientSecret"] = "secret",
                ["CrmAuthority"] = "authority",
                ["ServiceBusQueueName"] = "crmpluginjobs",
                ["ServiceBusLabel"] = "TINGLYSNINGDATO"
            }));

            try
            {
                EjendomTjekEjerskifteSettingsValidator.Validate(settings);
                Assert.Fail("Expected ConfigurationErrorsException was not thrown.");
            }
            catch (ConfigurationErrorsException)
            {
            }
        }
    }
}
