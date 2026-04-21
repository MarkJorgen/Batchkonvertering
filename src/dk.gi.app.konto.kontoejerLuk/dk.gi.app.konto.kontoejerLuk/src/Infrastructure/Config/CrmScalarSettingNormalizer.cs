namespace dk.gi.app.konto.kontoejerLuk.Infrastructure.Config
{
    public static class CrmScalarSettingNormalizer
    {
        public static string Normalize(string value)
        {
            return Gi.Batch.Shared.Configuration.CrmScalarSettingNormalizer.Normalize(value);
        }

        public static bool WasNormalized(string original, string normalized)
        {
            return Gi.Batch.Shared.Configuration.CrmScalarSettingNormalizer.WasNormalized(original, normalized);
        }

        public static bool HasOuterQuotes(string value)
        {
            return Gi.Batch.Shared.Configuration.CrmScalarSettingNormalizer.HasOuterQuotes(value);
        }

        public static bool ContainsLineBreaks(string value)
        {
            return Gi.Batch.Shared.Configuration.CrmScalarSettingNormalizer.ContainsLineBreaks(value);
        }

        public static bool LooksLikeGuid(string value)
        {
            return Gi.Batch.Shared.Configuration.CrmScalarSettingNormalizer.LooksLikeGuid(value);
        }
    }
}
