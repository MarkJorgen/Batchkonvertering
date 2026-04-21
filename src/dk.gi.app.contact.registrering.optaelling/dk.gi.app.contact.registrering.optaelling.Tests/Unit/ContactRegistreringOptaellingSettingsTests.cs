using System.Collections.Generic;
using System.Configuration;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Config;
using Gi.Batch.Shared.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registrering.optaelling.Tests.Unit
{
    [TestClass]
    public class ContactRegistreringOptaellingSettingsTests
    {
        [TestMethod]
        public void Create_Defaults_To_DryRun_And_Default_Wait_Values()
        {
            var configuration = new JobConfiguration(new Dictionary<string, string>());

            ContactRegistreringOptaellingSettings settings = ContactRegistreringOptaellingSettings.Create(configuration);

            Assert.AreEqual("DRYRUN", settings.Mode);
            Assert.IsTrue(settings.DryRun);
            Assert.AreEqual(25, settings.SecondsToSleep);
            Assert.AreEqual(5, settings.MaxWaitCount);
            Assert.IsFalse(settings.EnableLocalDebugLogging);
            Assert.AreEqual(string.Empty, settings.LocalDebugLogPath);
        }

        [TestMethod]
        public void Create_Maps_Crm_Settings()
        {
            var configuration = new JobConfiguration(new Dictionary<string, string>
            {
                ["Mode"] = "RUN",
                ["CrmConnectionTemplate"] = "template",
                ["CrmServerName"] = "server",
                ["CrmClientId"] = "client",
                ["CrmClientSecret"] = "secret",
                ["CrmAuthority"] = "authority",
                ["CrmAuthorityMode"] = "TenantBase"
            });

            ContactRegistreringOptaellingSettings settings = ContactRegistreringOptaellingSettings.Create(configuration);

            Assert.AreEqual("RUN", settings.Mode);
            Assert.IsFalse(settings.DryRun);
            Assert.AreEqual("server", settings.CrmServerName);
            Assert.AreEqual("client", settings.CrmClientId);
            Assert.AreEqual("TenantBase", settings.CrmAuthorityMode);
        }

        [TestMethod]
        public void Create_Sets_VerifyCrmOnly_When_Mode_Is_VERIFYCRM()
        {
            var configuration = new JobConfiguration(new Dictionary<string, string>
            {
                ["Mode"] = "VERIFYCRM"
            });

            ContactRegistreringOptaellingSettings settings = ContactRegistreringOptaellingSettings.Create(configuration);

            Assert.IsTrue(settings.VerifyCrmOnly);
            Assert.IsFalse(settings.DryRun);
        }


        [TestMethod]
        public void Create_Normalizes_Quoted_And_Trimmed_Crm_Scalar_Settings()
        {
            var configuration = new JobConfiguration(new Dictionary<string, string>
            {
                ["CrmServerName"] = "  \"gicrmdev.crm4.dynamics.com\"  ",
                ["CrmClientId"] = "  \"90ea86ce-6853-42f3-a23a-a0097eda830d\"  ",
                ["CrmClientSecret"] = "  \"secret-value\"  ",
                ["CrmAuthority"] = "  \"d5356f0d-2d9d-4c6c-86ed-f15d0c7f72e7\"  "
            });

            ContactRegistreringOptaellingSettings settings = ContactRegistreringOptaellingSettings.Create(configuration);

            Assert.AreEqual("gicrmdev.crm4.dynamics.com", settings.CrmServerName);
            Assert.AreEqual("90ea86ce-6853-42f3-a23a-a0097eda830d", settings.CrmClientId);
            Assert.AreEqual("secret-value", settings.CrmClientSecret);
            Assert.AreEqual("d5356f0d-2d9d-4c6c-86ed-f15d0c7f72e7", settings.CrmAuthority);
        }

        [TestMethod]
        public void StartupDiagnostics_Expose_Secret_Normalization_Shape_Without_Secret_Value()
        {
            var rawConfiguration = new JobConfiguration(new Dictionary<string, string>
            {
                ["CrmClientSecret"] = "  \"12345678-1234-1234-1234-123456789012\"  "
            });

            var settings = ContactRegistreringOptaellingSettings.Create(rawConfiguration);

            var diagnostics = ContactRegistreringStartupDiagnostics.Build(rawConfiguration, settings);
            string line = string.Join("\n", diagnostics);

            StringAssert.Contains(line, "CrmClientSecret normalized changed=Ja");
            StringAssert.Contains(line, "outer quotes raw=Nej");
            StringAssert.Contains(line, "looks like guid raw=Nej");
            StringAssert.Contains(line, "looks like guid normalized=Ja");
            StringAssert.Contains(line, "compat decrypt applied=Nej");
        }

        [TestMethod]
        public void Validate_Accepts_Empty_Local_Log_Path_When_Local_Logging_Is_Disabled()
        {
            var settings = ContactRegistreringOptaellingSettingsBuilder.Build(enableLocalDebugLogging: false, localDebugLogPath: string.Empty);

            ContactRegistreringOptaellingSettingsValidator.Validate(settings);
        }

        [TestMethod]
        public void Validate_Requires_Local_Log_Path_When_Local_Logging_Is_Enabled()
        {
            var settings = ContactRegistreringOptaellingSettingsBuilder.Build(enableLocalDebugLogging: true, localDebugLogPath: string.Empty);

            try
            {
                ContactRegistreringOptaellingSettingsValidator.Validate(settings);
                Assert.Fail("Expected ConfigurationErrorsException.");
            }
            catch (ConfigurationErrorsException ex)
            {
                StringAssert.Contains(ex.Message, "LocalDebugLogPath");
            }
        }
    }
}
