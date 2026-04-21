using System;
using dk.gi.app.contact.registrering.optaelling.Infrastructure.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registrering.optaelling.Tests.Smoke
{
    [TestClass]
    public class ServiceRegistrySmokeTests
    {
        [TestMethod]
        public void Build_Creates_Orchestrator_For_DryRun()
        {
            var args = new[] { "Mode=DRYRUN" };

            var result = ServiceRegistry.Build(args);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Orchestrator);
        }

        [TestMethod]
        public void Build_Throws_On_Invalid_RegistreringId()
        {
            try
            {
                ServiceRegistry.Build(new[] { "Mode=DRYRUN", "registreringid=ikke-en-guid" });
                Assert.Fail("Expected InvalidOperationException was not thrown.");
            }
            catch (InvalidOperationException)
            {
                // Expected.
            }
        }
    }
}
