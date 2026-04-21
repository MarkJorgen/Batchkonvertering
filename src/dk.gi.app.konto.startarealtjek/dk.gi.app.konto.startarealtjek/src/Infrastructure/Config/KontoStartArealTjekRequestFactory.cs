using dk.gi.app.konto.startarealtjek.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.konto.startarealtjek.Infrastructure.Config
{
    public static class KontoStartArealTjekRequestFactory
    {
        public static KontoStartArealTjekRequest Create(JobConfiguration configuration, KontoStartArealTjekSettings settings)
        {
            string kontoNr = string.Empty;
            if (configuration.TryGet("kontonr", out var rawValue) && string.IsNullOrWhiteSpace(rawValue) == false)
            {
                kontoNr = rawValue.Trim();
            }

            return new KontoStartArealTjekRequest(kontoNr, settings.Mode, settings.RunMode);
        }
    }
}
