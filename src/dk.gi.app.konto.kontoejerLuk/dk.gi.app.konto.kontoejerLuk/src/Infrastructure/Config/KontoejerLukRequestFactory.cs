using dk.gi.app.konto.kontoejerLuk.Application.Models;
using Gi.Batch.Shared.Configuration;

namespace dk.gi.app.konto.kontoejerLuk.Infrastructure.Config
{
    public static class KontoejerLukRequestFactory
    {
        public static KontoejerLukRequest Create(JobConfiguration configuration, KontoejerLukSettings settings)
            => new KontoejerLukRequest(settings.Mode, settings.RunMode);
    }
}
