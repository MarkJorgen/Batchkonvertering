using System;

namespace Gi.Batch.Shared.Configuration
{
    public static class CrmScalarSettingNormalizer
    {
        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string normalized = value.Trim();

            if (HasOuterQuotes(normalized))
            {
                normalized = normalized.Substring(1, normalized.Length - 2).Trim();
            }

            return normalized;
        }

        public static bool WasNormalized(string original, string normalized)
        {
            return !string.Equals(original ?? string.Empty, normalized ?? string.Empty, StringComparison.Ordinal);
        }

        public static bool HasOuterQuotes(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return value.Length >= 2 && value.StartsWith("\"") && value.EndsWith("\"");
        }

        public static bool ContainsLineBreaks(string value)
        {
            return !string.IsNullOrEmpty(value) && (value.Contains("\r") || value.Contains("\n"));
        }

        public static bool LooksLikeGuid(string value)
        {
            return Guid.TryParse(value, out _);
        }
    }
}
