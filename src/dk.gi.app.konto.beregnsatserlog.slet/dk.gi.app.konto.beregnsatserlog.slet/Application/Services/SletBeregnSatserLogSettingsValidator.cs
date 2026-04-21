using System;
using dk.gi.app.konto.beregnsatserlog.slet.Application.Models;

namespace dk.gi.app.konto.beregnsatserlog.slet.Application.Services
{
    public sealed class SletBeregnSatserLogSettingsValidator
    {
        public void ValidateAndThrow(SletBeregnSatserLogSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (settings.AntalAar <= 0 || settings.AntalAar > 50)
            {
                throw new InvalidOperationException("AntalAar skal ligge mellem 1 og 50.");
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
