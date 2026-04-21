using System;
using dk.gi.app.konto.satser.slet.Application.Models;

namespace dk.gi.app.konto.satser.slet.Application.Services
{
    public sealed class SletSatserSettingsValidator
    {
        public void ValidateAndThrow(SletSatserSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (settings.SatsAar < 2000 || settings.SatsAar > 2100)
            {
                throw new InvalidOperationException("SatsAar skal ligge mellem 2000 og 2100.");
            }

            if (settings.TimeOutMinutter <= 0 || settings.TimeOutMinutter > 60)
            {
                throw new InvalidOperationException("TimeOutMinutter skal ligge mellem 1 og 60.");
            }

            if (settings.SecondsToSleep < 0)
            {
                throw new InvalidOperationException("SecondsToSleep må ikke være negativ.");
            }

            if (settings.MaxWaitCount < 0)
            {
                throw new InvalidOperationException("MaxWaitCount må ikke være negativ.");
            }

            if (string.IsNullOrWhiteSpace(settings.CrmConnectionTemplate))
            {
                throw new InvalidOperationException("CrmConnectionTemplate er obligatorisk.");
            }
        }
    }
}
