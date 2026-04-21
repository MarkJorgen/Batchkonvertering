using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using dk.gi.app.konto.regnskab.slet.Application.Models;
using Microsoft.Extensions.Logging;

namespace dk.gi.app.konto.regnskab.slet.Infrastructure.Messaging
{
    public class RegnskabSletServiceBusSender
    {
        private readonly RegnskabSletSettings _settings;
        private readonly ILogger _logger;

        public RegnskabSletServiceBusSender(RegnskabSletSettings settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public virtual bool Send(KontoCandidate candidate, ResolvedServiceBusSettings resolvedServiceBusSettings, int scheduleDelaySeconds)
        {
            var effective = GetEffectiveSettings(resolvedServiceBusSettings);
            if (!effective.IsConfigured)
            {
                _logger.LogError("Service Bus settings mangler. Job kan ikke publiceres. Kilde={Source}", effective.Source);
                return false;
            }

            string endpoint = effective.BaseUrl.TrimEnd('/') + "/" + _settings.ServiceBusQueueName.Trim('/') + "/messages";
            string resourceUri = effective.BaseUrl.TrimEnd('/') + "/" + _settings.ServiceBusQueueName.Trim('/');
            string payload = RegnskabSletPayloadFactory.Create(candidate);
            string token = BuildSasToken(resourceUri, effective.SasKeyName, effective.SasKey);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", token.Substring("SharedAccessSignature ".Length));
                request.Headers.TryAddWithoutValidation("BrokerProperties", BuildBrokerProperties(scheduleDelaySeconds));
                request.Content = new StringContent(payload, Encoding.UTF8, "text/html");

                var response = client.SendAsync(request).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Publicerede Service Bus-job for konto {AccountNumber} ({AccountId}) via {Source}.", candidate.AccountNumber, candidate.AccountId, effective.Source);
                    return true;
                }

                string body = response.Content != null ? response.Content.ReadAsStringAsync().GetAwaiter().GetResult() : string.Empty;
                _logger.LogError("Service Bus-publicering fejlede for konto {AccountNumber}. Status={Status} Reason={Reason}. Kilde={Source}. Body={Body}", candidate.AccountNumber, (int)response.StatusCode, response.ReasonPhrase, effective.Source, body);
                return false;
            }
        }

        private ResolvedServiceBusSettings GetEffectiveSettings(ResolvedServiceBusSettings resolved)
        {
            bool localConfigured = !string.IsNullOrWhiteSpace(_settings.ServiceBusBaseUrl)
                && !string.IsNullOrWhiteSpace(_settings.ServiceBusSasKeyName)
                && !string.IsNullOrWhiteSpace(_settings.ServiceBusSasKey);
            if (localConfigured)
            {
                return new ResolvedServiceBusSettings(_settings.ServiceBusBaseUrl, _settings.ServiceBusSasKeyName, _settings.ServiceBusSasKey, "job settings");
            }

            return resolved ?? ResolvedServiceBusSettings.Empty("none");
        }

        private string BuildBrokerProperties(int scheduleDelaySeconds)
        {
            string messageId = Guid.NewGuid().ToString().ToUpperInvariant();
            string scheduled = string.Empty;
            if (scheduleDelaySeconds > 0)
            {
                DateTime enqueueTime = DateTime.UtcNow.AddSeconds(scheduleDelaySeconds);
                scheduled = ",\"ScheduledEnqueueTimeUtc\":\"" + enqueueTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.FFFZ") + "\"";
            }

            string sessionSegment = string.Empty;
            if (!string.IsNullOrWhiteSpace(_settings.ServiceBusSessionId))
            {
                sessionSegment = "\"SessionId\":\"" + _settings.ServiceBusSessionId + "\",";
            }

            return "{" + sessionSegment + "\"Label\":\"" + _settings.ServiceBusLabel + "\",\"MessageId\":\"" + messageId + "\"" + scheduled + "}";
        }

        private static string BuildSasToken(string resourceUri, string sasKeyName, string sasKey)
        {
            long expiry = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
            string encodedResourceUri = Uri.EscapeDataString(resourceUri);
            string toSign = encodedResourceUri + "\n" + expiry;

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sasKey)))
            {
                string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(toSign)));
                return "SharedAccessSignature sr=" + encodedResourceUri
                    + "&sig=" + Uri.EscapeDataString(signature)
                    + "&se=" + expiry
                    + "&skn=" + Uri.EscapeDataString(sasKeyName);
            }
        }
    }
}
