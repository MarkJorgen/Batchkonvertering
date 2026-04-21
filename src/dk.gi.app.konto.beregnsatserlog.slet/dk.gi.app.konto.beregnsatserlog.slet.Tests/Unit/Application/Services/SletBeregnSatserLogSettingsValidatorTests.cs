using System;
using dk.gi.app.konto.beregnsatserlog.slet.Application.Models;
using dk.gi.app.konto.beregnsatserlog.slet.Application.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.konto.beregnsatserlog.slet.Tests.Unit.Application.Services
{
    [TestClass]
    public class SletBeregnSatserLogSettingsValidatorTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateAndThrow_RejectsNonPositiveAntalAar()
        {
            var validator = new SletBeregnSatserLogSettingsValidator();
            validator.ValidateAndThrow(new SletBeregnSatserLogSettings
            {
                AntalAar = 0,
                TimeOutMinutter = 2,
                CrmConnectionTemplate = "x",
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateAndThrow_RejectsMissingConnectionTemplate()
        {
            var validator = new SletBeregnSatserLogSettingsValidator();
            validator.ValidateAndThrow(new SletBeregnSatserLogSettings
            {
                AntalAar = 3,
                TimeOutMinutter = 2,
            });
        }
    }
}
