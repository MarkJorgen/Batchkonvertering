using System.Collections.Generic;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dk.gi.app.contact.lassox.ophoer.Application.Models;
using dk.gi.app.contact.lassox.ophoer.Infrastructure.Config;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.contact.lassox.ophoer.Tests.Unit
{
    [TestClass]
    public class LassoXOphoerSettingsValidatorTests
    {
        [TestMethod]
        public void Validate_DryRun_MedCrmSettings_Bestaar()
        {
            var settings = LassoXOphoerSettings.Create(new JobConfiguration(new Dictionary<string, string>
            {
                ["Mode"] = "DRYRUN",
                ["CrmConnectionTemplate"] = "AuthType=ClientSecret;Url=https://{0};ClientId={1};ClientSecret={2};Authority=https://{3};",
                ["CrmServerName"] = "server",
                ["CrmClientId"] = "client",
                ["CrmClientSecret"] = "secret",
                ["CrmAuthority"] = "tenant"
            }));

            LassoXOphoerSettingsValidator.Validate(settings);
        }

        [TestMethod]
        public void Validate_Run_UdenCrmSettings_Fejer()
        {
            var settings = LassoXOphoerSettings.Create(new JobConfiguration(new Dictionary<string, string>
            {
                ["Mode"] = "RUN"
            }));

            try
            {
                LassoXOphoerSettingsValidator.Validate(settings);
                Assert.Fail("Expected ConfigurationErrorsException was not thrown.");
            }
            catch (ConfigurationErrorsException)
            {
                // expected
            }
        }
    }
}
