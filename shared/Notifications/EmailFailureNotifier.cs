using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Gi.Batch.Shared.Configuration;

namespace Gi.Batch.Shared.Notifications
{
    public sealed class EmailFailureNotifier
    {
        private readonly bool _enabled;
        private readonly string _failureRecipients;
        private readonly string _emailUrl;
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _senderEmail;
        private readonly ConsoleFailureNotifier _fallback = new ConsoleFailureNotifier();

        public EmailFailureNotifier(JobConfiguration configuration, string failureRecipients)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _enabled = configuration.GetBool("EnableFailureEmail", false);
            _failureRecipients = failureRecipients ?? string.Empty;
            _emailUrl = (configuration.Get("Azure.Email.Url", string.Empty) ?? string.Empty).Trim();
            _tenantId = (configuration.Get("Azure.Email.TenantID", string.Empty) ?? string.Empty).Trim();
            _clientId = (configuration.Get("Azure.Email.ClientID", string.Empty) ?? string.Empty).Trim();
            _clientSecret = (configuration.Get("Azure.Email.ClientSecret", string.Empty) ?? string.Empty).Trim();
            _senderEmail = (configuration.Get("Azure.Email.AfsenderEmail", string.Empty) ?? string.Empty).Trim();
        }

        public void Notify(string subject, string message, Exception exception)
        {
            if (!_enabled)
            {
                _fallback.Notify(subject, Decorate(message, exception, "[MAIL] Failure email disabled; console fallback used."), exception);
                return;
            }

            if (!HasRequiredConfiguration())
            {
                _fallback.Notify(subject, Decorate(message, exception, "[MAIL] Failure email enabled, but required mail configuration is incomplete; console fallback used."), exception);
                return;
            }

            var recipients = SplitRecipients(_failureRecipients);
            if (recipients.Count == 0)
            {
                _fallback.Notify(subject, Decorate(message, exception, "[MAIL] Failure email enabled, but modtagereEmail is empty; console fallback used."), exception);
                return;
            }

            try
            {
                Send(subject ?? "Batchjob failure", BuildBody(message, exception), recipients);
            }
            catch (Exception sendException)
            {
                _fallback.Notify(subject,
                    Decorate(message, exception, "[MAIL] Failure email send failed; console fallback used. " + sendException.Message),
                    sendException);
            }
        }

        private bool HasRequiredConfiguration()
        {
            return !string.IsNullOrWhiteSpace(_emailUrl)
                && !string.IsNullOrWhiteSpace(_tenantId)
                && !string.IsNullOrWhiteSpace(_clientId)
                && !string.IsNullOrWhiteSpace(_clientSecret)
                && !string.IsNullOrWhiteSpace(_senderEmail);
        }

        private void Send(string subject, string body, IReadOnlyList<string> recipients)
        {
            using (var client = new HttpClient())
            {
                string token = AcquireAccessToken(client);
                string endpoint = ResolveEmailUrl();
                string payload = BuildSendMailJson(subject, body, recipients);

                using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                    using (var response = client.SendAsync(request).GetAwaiter().GetResult())
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            string responseBody = response.Content == null
                                ? string.Empty
                                : response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                            throw new InvalidOperationException(
                                "Mail send failed with status " + (int)response.StatusCode + " " + response.ReasonPhrase + ". " + responseBody);
                        }
                    }
                }
            }
        }

        private string AcquireAccessToken(HttpClient client)
        {
            string tokenEndpoint = "https://login.microsoftonline.com/" + Uri.EscapeDataString(_tenantId) + "/oauth2/v2.0/token";

            using (var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint))
            {
                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _clientSecret),
                    new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                using (var response = client.SendAsync(request).GetAwaiter().GetResult())
                {
                    string body = response.Content == null
                        ? string.Empty
                        : response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "Token acquisition failed with status " + (int)response.StatusCode + " " + response.ReasonPhrase + ". " + body);
                    }

                    string token = ExtractJsonString(body, "access_token");
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        throw new InvalidOperationException("Token acquisition succeeded, but access_token was missing in response.");
                    }

                    return token;
                }
            }
        }

        private string ResolveEmailUrl()
        {
            return _emailUrl.Contains("{0}")
                ? string.Format(_emailUrl, _senderEmail)
                : _emailUrl;
        }

        private static IReadOnlyList<string> SplitRecipients(string recipients)
        {
            return (recipients ?? string.Empty)
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(r => (r ?? string.Empty).Trim().Trim('"'))
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string BuildSendMailJson(string subject, string body, IReadOnlyList<string> recipients)
        {
            var recipientsJson = string.Join(",", recipients.Select(r => "{\"emailAddress\":{\"address\":\"" + JsonEscape(r) + "\"}}"));

            return "{" +
                   "\"message\":{" +
                   "\"subject\":\"" + JsonEscape(subject ?? string.Empty) + "\"," +
                   "\"body\":{\"contentType\":\"Text\",\"content\":\"" + JsonEscape(body ?? string.Empty) + "\"}," +
                   "\"toRecipients\":[" + recipientsJson + "]" +
                   "}," +
                   "\"saveToSentItems\":false" +
                   "}";
        }

        private static string BuildBody(string message, Exception exception)
        {
            var sb = new StringBuilder();
            sb.AppendLine(message ?? string.Empty);
            if (exception != null)
            {
                sb.AppendLine();
                sb.AppendLine("Exception:");
                sb.AppendLine(exception.ToString());
            }

            return sb.ToString().Trim();
        }

        private static string Decorate(string message, Exception exception, string prefix)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                sb.AppendLine(prefix);
            }

            sb.AppendLine(message ?? string.Empty);
            if (exception != null)
            {
                sb.AppendLine();
                sb.AppendLine("Exception:");
                sb.AppendLine(exception.ToString());
            }

            return sb.ToString().Trim();
        }

        private static string ExtractJsonString(string json, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(propertyName))
            {
                return string.Empty;
            }

            string marker = "\"" + propertyName + "\"";
            int markerIndex = json.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
            {
                return string.Empty;
            }

            int colonIndex = json.IndexOf(':', markerIndex + marker.Length);
            if (colonIndex < 0)
            {
                return string.Empty;
            }

            int quoteStart = json.IndexOf('"', colonIndex + 1);
            if (quoteStart < 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            bool escaping = false;
            for (int i = quoteStart + 1; i < json.Length; i++)
            {
                char ch = json[i];
                if (escaping)
                {
                    switch (ch)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        default: sb.Append(ch); break;
                    }

                    escaping = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escaping = true;
                    continue;
                }

                if (ch == '"')
                {
                    return sb.ToString();
                }

                sb.Append(ch);
            }

            return string.Empty;
        }

        private static string JsonEscape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(value.Length + 16);
            foreach (char ch in value)
            {
                switch (ch)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    default:
                        if (char.IsControl(ch))
                        {
                            sb.Append("\\u");
                            sb.Append(((int)ch).ToString("x4"));
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
