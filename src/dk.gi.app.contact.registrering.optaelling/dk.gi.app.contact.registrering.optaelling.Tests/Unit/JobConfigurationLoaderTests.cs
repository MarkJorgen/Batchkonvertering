using System;
using System.Collections.Generic;
using System.IO;
using Gi.Batch.Shared.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registrering.optaelling.Tests.Unit
{
    [TestClass]
    public class JobConfigurationLoaderTests
    {
        [TestMethod]
        public void Load_Uses_Azure_When_ConfigStore_Is_Enabled_And_No_LocalOverrideFile_Exists()
        {
            SetEnvironmentVariable("UseConfigStore", "true");
            SetEnvironmentVariable("AZURE_APPCONFIG_CONNECTIONSTRING", "Endpoint=fake");
            SetEnvironmentVariable("LocalOverrideFile", "does-not-exist.local.json");
            SetEnvironmentVariable("modtagereEmail", null);

            try
            {
                var loader = new JobConfigurationLoader(new StubAzureSettingsSource(new Dictionary<string, string>
                {
                    ["modtagereEmail"] = "drift@gi.dk"
                }));

                JobConfiguration configuration = loader.Load(new string[0]);

                Assert.AreEqual("drift@gi.dk", configuration.Get("modtagereEmail"));
            }
            finally
            {
                SetEnvironmentVariable("UseConfigStore", null);
                SetEnvironmentVariable("AZURE_APPCONFIG_CONNECTIONSTRING", null);
                SetEnvironmentVariable("LocalOverrideFile", null);
                SetEnvironmentVariable("modtagereEmail", null);
            }
        }

        [TestMethod]
        public void Load_Uses_Azure_When_Azure_AppConfig_EnvVar_Is_Set_Without_Explicit_UseConfigStore()
        {
            SetEnvironmentVariable("UseConfigStore", null);
            SetEnvironmentVariable("AZURE_APPCONFIG_CONNECTIONSTRING", "Endpoint=fake");
            SetEnvironmentVariable("LocalOverrideFile", "does-not-exist.local.json");
            SetEnvironmentVariable("modtagereEmail", null);

            try
            {
                var loader = new JobConfigurationLoader(new StubAzureSettingsSource(new Dictionary<string, string>
                {
                    ["modtagereEmail"] = "drift-auto@gi.dk"
                }));

                JobConfiguration configuration = loader.Load(new string[0]);

                Assert.AreEqual("drift-auto@gi.dk", configuration.Get("modtagereEmail"));
            }
            finally
            {
                SetEnvironmentVariable("UseConfigStore", null);
                SetEnvironmentVariable("AZURE_APPCONFIG_CONNECTIONSTRING", null);
                SetEnvironmentVariable("LocalOverrideFile", null);
                SetEnvironmentVariable("modtagereEmail", null);
            }
        }

        [TestMethod]
        public void Load_Uses_Local_Json_When_LocalOverrideFile_Exists()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.local.test.json");
            File.WriteAllText(path, "{\"modtagereEmail\":\"mark+dev@gi.dk\"}");

            SetEnvironmentVariable("UseConfigStore", "true");
            SetEnvironmentVariable("AZURE_APPCONFIG_CONNECTIONSTRING", "Endpoint=fake");
            SetEnvironmentVariable("LocalOverrideFile", "appsettings.local.test.json");

            try
            {
                var loader = new JobConfigurationLoader(new StubAzureSettingsSource(new Dictionary<string, string>
                {
                    ["modtagereEmail"] = "drift@gi.dk"
                }));

                JobConfiguration configuration = loader.Load(new string[0]);

                Assert.AreEqual("mark+dev@gi.dk", configuration.Get("modtagereEmail"));
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                SetEnvironmentVariable("UseConfigStore", null);
                SetEnvironmentVariable("AZURE_APPCONFIG_CONNECTIONSTRING", null);
                SetEnvironmentVariable("LocalOverrideFile", null);
            }
        }

        [TestMethod]
        public void Load_Args_Override_Local_Json()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.local.test.json");
            File.WriteAllText(path, "{\"Mode\":\"DRYRUN\"}");

            SetEnvironmentVariable("LocalOverrideFile", "appsettings.local.test.json");

            try
            {
                var loader = new JobConfigurationLoader(new StubAzureSettingsSource(new Dictionary<string, string>()));
                JobConfiguration configuration = loader.Load(new[] { "Mode=RUN" });

                Assert.AreEqual("RUN", configuration.Get("Mode"));
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                SetEnvironmentVariable("LocalOverrideFile", null);
            }
        }

        [TestMethod]
        public void Load_Does_Not_Let_Empty_Environment_Value_Override_Azure_Value()
        {
            SetEnvironmentVariable("UseConfigStore", null);
            SetEnvironmentVariable("AZURE_APPCONFIG_CONNECTIONSTRING", "Endpoint=fake");
            SetEnvironmentVariable("LocalOverrideFile", "does-not-exist.local.json");
            SetEnvironmentVariable("modtagereEmail", string.Empty);

            try
            {
                var loader = new JobConfigurationLoader(new StubAzureSettingsSource(new Dictionary<string, string>
                {
                    ["modtagereEmail"] = "drift-empty-env@gi.dk"
                }));

                JobConfiguration configuration = loader.Load(new string[0]);

                Assert.AreEqual("drift-empty-env@gi.dk", configuration.Get("modtagereEmail"));
            }
            finally
            {
                SetEnvironmentVariable("UseConfigStore", null);
                SetEnvironmentVariable("AZURE_APPCONFIG_CONNECTIONSTRING", null);
                SetEnvironmentVariable("LocalOverrideFile", null);
                SetEnvironmentVariable("modtagereEmail", null);
            }
        }

        [TestMethod]
        public void Load_Explicit_UseConfigStore_False_In_Environment_Disables_Azure_AutoBootstrap()
        {
            SetEnvironmentVariable("UseConfigStore", "false");
            SetEnvironmentVariable("AZURE_APPCONFIG_CONNECTIONSTRING", "Endpoint=fake");
            SetEnvironmentVariable("LocalOverrideFile", "does-not-exist.local.json");
            SetEnvironmentVariable("modtagereEmail", null);

            try
            {
                var loader = new JobConfigurationLoader(new StubAzureSettingsSource(new Dictionary<string, string>
                {
                    ["modtagereEmail"] = "should-not-win@gi.dk"
                }));

                JobConfiguration configuration = loader.Load(new string[0]);

                Assert.AreEqual(null, configuration.Get("modtagereEmail"));
            }
            finally
            {
                SetEnvironmentVariable("UseConfigStore", null);
                SetEnvironmentVariable("AZURE_APPCONFIG_CONNECTIONSTRING", null);
                SetEnvironmentVariable("LocalOverrideFile", null);
                SetEnvironmentVariable("modtagereEmail", null);
            }
        }

        private static void SetEnvironmentVariable(string key, string value)
        {
            Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);
        }

        private sealed class StubAzureSettingsSource : IAzureSettingsSource
        {
            private readonly IReadOnlyDictionary<string, string> _values;

            public StubAzureSettingsSource(IReadOnlyDictionary<string, string> values)
            {
                _values = values;
            }

            public IReadOnlyDictionary<string, string> Load(string connectionString)
            {
                return _values;
            }
        }
    }
}
