using System.Collections.Generic;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dk.gi.app.konto.afslutarealsager.Application.Models;
using dk.gi.app.konto.afslutarealsager.Infrastructure.Config;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.konto.afslutarealsager.Tests.Unit
{
    [TestClass]
    public class KontoAfslutArealSagerSettingsValidatorTests
    {
        [TestMethod]
        public void Validate_With_Missing_BrugerArealSager_Throws()
        {
            var configuration = new JobConfiguration(new Dictionary<string, string>
            {
                ["Mode"] = "DRYRUN",
                ["CrmConnectionTemplate"] = "AuthType=ClientSecret;Url=https://{0};ClientId={1};ClientSecret={2};Authority=https://{3};",
                ["CrmServerName"] = "server",
                ["CrmClientId"] = "client",
                ["CrmClientSecret"] = "secret",
                ["CrmAuthority"] = "tenant"
            });

            var settings = KontoAfslutArealSagerSettings.Create(configuration);

            try
            {
                KontoAfslutArealSagerSettingsValidator.Validate(settings);
                Assert.Fail("Expected ConfigurationErrorsException was not thrown.");
            }
            catch (ConfigurationErrorsException)
            {
            }
        }
    }
}
