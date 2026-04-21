using System.Configuration;
using Gi.Batch.Shared.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registrering.optaelling.Tests.Regression
{
    [TestClass]
    public class ConfigStoreRegressionTests
    {
        [TestMethod]
        public void Load_Throws_When_ConfigStore_Is_Enabled_Without_ConnectionString()
        {
            System.Environment.SetEnvironmentVariable("UseConfigStore", "true", System.EnvironmentVariableTarget.Process);
            System.Environment.SetEnvironmentVariable("AZURE_APPCONFIG_CONNECTIONSTRING", null, System.EnvironmentVariableTarget.Process);

            try
            {
                var loader = new JobConfigurationLoader(new StubAzureSettingsSource());

                try
                {
                    loader.Load(new string[0]);
                    Assert.Fail("Expected ConfigurationErrorsException was not thrown.");
                }
                catch (ConfigurationErrorsException ex)
                {
                    StringAssert.Contains(ex.Message, "AZURE_APPCONFIG_CONNECTIONSTRING");
                }
            }
            finally
            {
                System.Environment.SetEnvironmentVariable("UseConfigStore", null, System.EnvironmentVariableTarget.Process);
                System.Environment.SetEnvironmentVariable("AZURE_APPCONFIG_CONNECTIONSTRING", null, System.EnvironmentVariableTarget.Process);
            }
        }

        private sealed class StubAzureSettingsSource : IAzureSettingsSource
        {
            public System.Collections.Generic.IReadOnlyDictionary<string, string> Load(string connectionString)
            {
                return new System.Collections.Generic.Dictionary<string, string>();
            }
        }
    }
}
