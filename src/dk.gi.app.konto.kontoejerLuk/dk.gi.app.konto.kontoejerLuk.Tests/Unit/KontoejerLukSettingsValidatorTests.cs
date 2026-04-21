using System.Collections.Generic;
using System.Configuration;
using dk.gi.app.konto.kontoejerLuk.Application.Models;
using dk.gi.app.konto.kontoejerLuk.Infrastructure.Config;
using Gi.Batch.Shared.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.konto.kontoejerLuk.Tests.Unit
{
    [TestClass]
    public class KontoejerLukSettingsValidatorTests
    {
        [TestMethod]
        public void Validate_DoesNotThrow_ForMinimumValidSettings()
        {
            var settings = KontoejerLukSettings.Create(new JobConfiguration(new Dictionary<string, string>
            {
                ["Mode"] = "DRYRUN",
                ["CrmConnectionTemplate"] = "AuthType=ClientSecret;Url={0};ClientId={1};ClientSecret={2};Authority={3};",
                ["CrmServerName"] = "https://example.crm4.dynamics.com",
                ["CrmClientId"] = "client",
                ["CrmClientSecret"] = "secret",
                ["CrmAuthority"] = "authority"
            }));

            KontoejerLukSettingsValidator.Validate(settings);
        }

        [TestMethod]
        public void Validate_Throws_WhenCrmConnectionTemplateMissing()
        {
            var settings = KontoejerLukSettings.Create(new JobConfiguration(new Dictionary<string, string>
            {
                ["Mode"] = "DRYRUN",
                ["CrmServerName"] = "https://example.crm4.dynamics.com",
                ["CrmClientId"] = "client",
                ["CrmClientSecret"] = "secret",
                ["CrmAuthority"] = "authority"
            }));

            try
            {
                KontoejerLukSettingsValidator.Validate(settings);
                Assert.Fail("Expected ConfigurationErrorsException was not thrown.");
            }
            catch (ConfigurationErrorsException)
            {
            }
        }
    }
}
