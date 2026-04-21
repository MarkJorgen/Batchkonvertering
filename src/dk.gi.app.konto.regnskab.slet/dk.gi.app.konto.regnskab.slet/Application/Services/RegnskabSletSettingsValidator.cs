using System;
using dk.gi.app.konto.regnskab.slet.Application.Models;

namespace dk.gi.app.konto.regnskab.slet.Application.Services
{
    public sealed class RegnskabSletSettingsValidator
    {
        public void ValidateAndThrow(RegnskabSletSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.CrmConnectionTemplate)) throw new InvalidOperationException("CrmConnectionTemplate er obligatorisk.");
            if (string.IsNullOrWhiteSpace(settings.ServiceBusQueueName)) throw new InvalidOperationException("ServiceBusQueueName er obligatorisk.");
            if (string.IsNullOrWhiteSpace(settings.ServiceBusLabel)) throw new InvalidOperationException("ServiceBusLabel er obligatorisk.");
            if (settings.DelayStepSeconds < 0 || settings.DelayStepSeconds > 600) throw new InvalidOperationException("DelayStepSeconds skal ligge mellem 0 og 600.");
            if (settings.DefaultBatchCount <= 0 || settings.DefaultBatchCount > 10000) throw new InvalidOperationException("DefaultBatchCount skal ligge mellem 1 og 10000.");
        }
    }
}
