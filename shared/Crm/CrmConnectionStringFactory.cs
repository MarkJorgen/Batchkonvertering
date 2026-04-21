using System;
using System.Collections.Generic;
using System.Linq;

namespace Gi.Batch.Shared.Crm
{
    public static class CrmConnectionStringFactory
    {
        public static string Create(string connectionTemplate, string serverName, string clientId, string clientSecret, string authority, string authorityMode)
        {
            string secret = CompatCrmSecretDecryptor.DecryptOrFallback(clientSecret ?? string.Empty, out _);
            string connectionString = string.Format(connectionTemplate ?? string.Empty, serverName ?? string.Empty, clientId ?? string.Empty, secret, authority ?? string.Empty);
            return ApplyAuthorityMode(connectionString, authorityMode, authority);
        }

        public static string CreateSanitized(string connectionTemplate, string serverName, string clientId, string clientSecret, string authority, string authorityMode)
        {
            string sanitizedSecret = string.IsNullOrWhiteSpace(clientSecret) ? string.Empty : "***";
            string connectionString = string.Format(connectionTemplate ?? string.Empty, serverName ?? string.Empty, clientId ?? string.Empty, sanitizedSecret, authority ?? string.Empty);
            return ApplyAuthorityMode(connectionString, authorityMode, authority);
        }

        private static string ApplyAuthorityMode(string connectionString, string mode, string authority)
        {
            string effectiveMode = string.IsNullOrWhiteSpace(mode) ? "AsConfigured" : mode;

            if (string.Equals(effectiveMode, "AsConfigured", StringComparison.OrdinalIgnoreCase))
            {
                return connectionString;
            }

            if (string.Equals(effectiveMode, "Omit", StringComparison.OrdinalIgnoreCase))
            {
                return RemoveConnectionStringKey(connectionString, "Authority");
            }

            if (string.Equals(effectiveMode, "TenantBase", StringComparison.OrdinalIgnoreCase))
            {
                string normalizedAuthority = NormalizeAuthorityToTenantBase(authority);
                if (string.IsNullOrWhiteSpace(normalizedAuthority))
                {
                    return RemoveConnectionStringKey(connectionString, "Authority");
                }

                return SetConnectionStringKey(connectionString, "Authority", normalizedAuthority);
            }

            return connectionString;
        }

        private static string NormalizeAuthorityToTenantBase(string authority)
        {
            if (string.IsNullOrWhiteSpace(authority))
            {
                return string.Empty;
            }

            string trimmed = authority.Trim();
            if (Guid.TryParse(trimmed, out _))
            {
                return "https://login.microsoftonline.com/" + trimmed;
            }

            if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = "https://" + trimmed.TrimStart('/');
            }

            if (Uri.TryCreate(trimmed, UriKind.Absolute, out Uri uri))
            {
                string[] segments = uri.AbsolutePath.Trim('/').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length > 0)
                {
                    return uri.Scheme + "://" + uri.Host.TrimEnd('/') + "/" + segments[0];
                }

                return uri.GetLeftPart(UriPartial.Authority);
            }

            return trimmed;
        }

        private static string RemoveConnectionStringKey(string connectionString, string key)
        {
            return string.Join(";", ParseConnectionStringParts(connectionString)
                .Where(part => !string.Equals(part.Key, key, StringComparison.OrdinalIgnoreCase))
                .Select(part => part.Key + "=" + part.Value)) + ";";
        }

        private static string SetConnectionStringKey(string connectionString, string key, string value)
        {
            var parts = ParseConnectionStringParts(connectionString);
            bool replaced = false;

            for (int i = 0; i < parts.Count; i++)
            {
                if (string.Equals(parts[i].Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    parts[i] = new KeyValuePair<string, string>(parts[i].Key, value);
                    replaced = true;
                    break;
                }
            }

            if (!replaced)
            {
                parts.Add(new KeyValuePair<string, string>(key, value));
            }

            return string.Join(";", parts.Select(part => part.Key + "=" + part.Value)) + ";";
        }

        private static List<KeyValuePair<string, string>> ParseConnectionStringParts(string connectionString)
        {
            var parts = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return parts;
            }

            string[] tokens = connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                int separatorIndex = token.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = token.Substring(0, separatorIndex).Trim();
                string value = token.Substring(separatorIndex + 1).Trim();
                parts.Add(new KeyValuePair<string, string>(key, value));
            }

            return parts;
        }
    }
}
