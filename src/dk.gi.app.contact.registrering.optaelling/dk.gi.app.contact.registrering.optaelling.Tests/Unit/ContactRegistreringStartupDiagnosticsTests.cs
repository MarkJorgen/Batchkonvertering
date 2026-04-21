using System.Collections.Generic;
using System.Linq;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Config;
using Gi.Batch.Shared.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registrering.optaelling.Tests.Unit
{
    [TestClass]
    public class ContactRegistreringStartupDiagnosticsTests
    {
        [TestMethod]
        public void Build_Reports_Missing_Crm_Settings_In_Run_Mode()
        {
            var rawConfiguration = new JobConfiguration(new Dictionary<string, string>
            {
                ["UseConfigStore"] = "true",
                ["LocalOverrideFile"] = "appsettings.local.json"
            });

            var settings = ContactRegistreringOptaellingSettings.Create(new JobConfiguration(new Dictionary<string, string>
            {
                ["Mode"] = "RUN",
                ["EnableLocalDebugLogging"] = "false",
                ["LocalDebugLogPath"] = "",
                ["MutexName"] = "test",
                ["SecondsToSleep"] = "1",
                ["MaxWaitCount"] = "1",
                ["modtagereEmail"] = "test@gi.dk",
                ["CrmConnectionTemplate"] = "",
                ["CrmServerName"] = "",
                ["CrmClientId"] = "",
                ["CrmClientSecret"] = "",
                ["CrmAuthority"] = ""
            }));

            var diagnostics = ContactRegistreringStartupDiagnostics.Build(rawConfiguration, settings);
            var lines = diagnostics.ToList();

            CollectionAssert.Contains(lines, "[DIAG] CrmConnectionTemplate=MISSING");
            CollectionAssert.Contains(lines, "[DIAG] CrmServerName=MISSING");
            CollectionAssert.Contains(lines, "[DIAG] CrmClientId=MISSING");
            CollectionAssert.Contains(lines, "[DIAG] CrmClientSecret=MISSING");
            CollectionAssert.Contains(lines, "[DIAG] CrmAuthority=MISSING");
        }

        [TestMethod]
        public void Build_Reports_Mode_And_ConfigStore_Usage()
        {
            var rawConfiguration = new JobConfiguration(new Dictionary<string, string>
            {
                ["UseConfigStore"] = "true"
            });

            ContactRegistreringOptaellingSettings settings = ContactRegistreringOptaellingSettingsBuilder.Build(mode: "DRYRUN");

            var diagnostics = ContactRegistreringStartupDiagnostics.Build(rawConfiguration, settings);
            var lines = diagnostics.ToList();

            CollectionAssert.Contains(lines, "[DIAG] Mode=DRYRUN");
            CollectionAssert.Contains(lines, "[DIAG] UseConfigStore=true");
        }
    }
}
