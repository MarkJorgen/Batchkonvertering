using System;
using dk.gi.app.konto.satser.slet.Application.Models;
using dk.gi.app.konto.satser.slet.Application.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.konto.satser.slet.Tests.Unit.Application.Services
{
    [TestClass]
    public class SletSatserSettingsValidatorTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateAndThrow_RejectsOutOfRangeSatsAar()
        {
            var validator = new SletSatserSettingsValidator();
            validator.ValidateAndThrow(new SletSatserSettings
            {
                SatsAar = 1999,
                TimeOutMinutter = 2,
                CrmConnectionTemplate = "x",
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateAndThrow_RejectsMissingConnectionTemplate()
        {
            var validator = new SletSatserSettingsValidator();
            validator.ValidateAndThrow(new SletSatserSettings
            {
                SatsAar = 2027,
                TimeOutMinutter = 2,
            });
        }
    }
}
