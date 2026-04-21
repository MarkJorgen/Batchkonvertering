using System;
using dk.gi.app.konto.satser.opret.Application.Models;

namespace dk.gi.app.konto.satser.opret.Application.Services
{
    public sealed class OpretSatserSettingsValidator
    {
        public void ValidateAndThrow(OpretSatserSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (settings.SatsAar < 2000 || settings.SatsAar > 3000)
            {
                throw new InvalidOperationException("SatsAar ligger uden for forventet interval.");
            }

            if (string.IsNullOrWhiteSpace(settings.CrmConnectionTemplate))
            {
                throw new InvalidOperationException("CrmConnectionTemplate mangler.");
            }

            if (string.IsNullOrWhiteSpace(settings.CrmServerName))
            {
                throw new InvalidOperationException("CrmServerName mangler.");
            }

            if (string.IsNullOrWhiteSpace(settings.CrmClientId))
            {
                throw new InvalidOperationException("CrmClientId mangler.");
            }

            if (string.IsNullOrWhiteSpace(settings.CrmClientSecret))
            {
                throw new InvalidOperationException("CrmClientSecret mangler.");
            }

            if (string.IsNullOrWhiteSpace(settings.CrmAuthority))
            {
                throw new InvalidOperationException("CrmAuthority mangler.");
            }
        }
    }
}
