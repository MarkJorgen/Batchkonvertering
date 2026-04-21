using System;
using dk.gi.app.konto.regnskab.slet.Application.Models;
using dk.gi.app.konto.regnskab.slet.Application.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.konto.regnskab.slet.Tests.Unit.Application.Services
{
    [TestClass]
    public class RegnskabSletSettingsValidatorTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateAndThrow_RejectsMissingQueueName()
        {
            var validator = new RegnskabSletSettingsValidator();
            validator.ValidateAndThrow(new RegnskabSletSettings { CrmConnectionTemplate = "x", ServiceBusLabel = "KontoDiv", DefaultBatchCount = 100, DelayStepSeconds = 15 });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateAndThrow_RejectsInvalidBatchCount()
        {
            var validator = new RegnskabSletSettingsValidator();
            validator.ValidateAndThrow(new RegnskabSletSettings { CrmConnectionTemplate = "x", ServiceBusQueueName = "crmpluginjobs", ServiceBusLabel = "KontoDiv", DefaultBatchCount = 0, DelayStepSeconds = 15 });
        }
    }
}
