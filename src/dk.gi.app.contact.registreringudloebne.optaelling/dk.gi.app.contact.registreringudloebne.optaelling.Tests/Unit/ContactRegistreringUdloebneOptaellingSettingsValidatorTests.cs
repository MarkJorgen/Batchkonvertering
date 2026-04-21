using dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Tests.Unit
{
    [TestClass]
    public class ContactRegistreringUdloebneOptaellingSettingsValidatorTests
    {
        [TestMethod]
        public void Validate_Allows_DryRun_Without_Crm_And_ServiceBus()
        {
            var settings = ContactRegistreringUdloebneOptaellingSettingsBuilder.Build("DRYRUN");
            ContactRegistreringUdloebneOptaellingSettingsValidator.Validate(settings);
        }

        [TestMethod]
        public void Validate_Allows_Run_Without_ServiceBus_In_Startup_When_Crm_Is_Configured()
        {
            var settings = ContactRegistreringUdloebneOptaellingSettingsBuilder.Build(
                "RUN",
                serviceBusBaseUrl: "",
                serviceBusSasKeyName: "",
                serviceBusSasKey: "",
                serviceBusQueueName: "",
                serviceBusLabel: "",
                serviceBusSessionId: "");

            ContactRegistreringUdloebneOptaellingSettingsValidator.Validate(settings);
        }
    }
}
