using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using dk.gi.app.ejendom.tjekejerskifte.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.ejendom.tjekejerskifte.Infrastructure.Messaging
{
    public class EjendomTjekEjerskifteServiceBusSender
    {
        private readonly EjendomTjekEjerskifteSettings _settings;
        private readonly IJobLogger _logger;

        public EjendomTjekEjerskifteServiceBusSender(EjendomTjekEjerskifteSettings settings, IJobLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public virtual bool Send(EjendomTjekEjerskifteCandidate candidate, ResolvedServiceBusSettings resolvedServiceBusSettings, int scheduleDelaySeconds)
        {
            var effective = GetEffectiveSettings(resolvedServiceBusSettings);

            if (!effective.IsConfigured)
            {
                _logger.Error("Service Bus settings mangler. Job kan ikke publiceres. Mangler=ServiceBusBaseUrl/ServiceBusSasKeyName/ServiceBusSasKey. Kilde=" + effective.Source);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_settings.ServiceBusQueueName)
                || string.IsNullOrWhiteSpace(_settings.ServiceBusLabel))
            {
                _logger.Error("Service Bus message settings mangler. Kræver mindst ServiceBusQueueName og ServiceBusLabel.");
                return false;
            }

            string endpoint = effective.BaseUrl.TrimEnd('/') + "/" + _settings.ServiceBusQueueName.Trim('/') + "/messages";
            string resourceUri = effective.BaseUrl.TrimEnd('/') + "/" + _settings.ServiceBusQueueName.Trim('/');
            string payload = EjendomTjekEjerskiftePayloadFactory.Create(candidate);
            string token = BuildSasToken(resourceUri, effective.SasKeyName, effective.SasKey);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", token.Substring("SharedAccessSignature ".Length));
                request.Headers.TryAddWithoutValidation("BrokerProperties", BuildBrokerProperties(scheduleDelaySeconds));
                request.Content = new StringContent(payload, Encoding.UTF8, "text/html");

                HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    _logger.Info("Publicerede Service Bus-job for ejendom " + candidate.PropertyId + " med BFEnummer=" + candidate.BfeNummer + " via " + effective.Source + ".");
                    return true;
                }

                string body = response.Content != null ? response.Content.ReadAsStringAsync().GetAwaiter().GetResult() : string.Empty;
                _logger.Error("Service Bus-publicering fejlede for ejendom " + candidate.PropertyId + ". Status=" + (int)response.StatusCode + " Reason=" + response.ReasonPhrase + ". Kilde=" + effective.Source + ". Body=" + body);
                return false;
            }
        }

        private ResolvedServiceBusSettings GetEffectiveSettings(ResolvedServiceBusSettings resolvedServiceBusSettings)
        {
            bool localConfigured = !string.IsNullOrWhiteSpace(_settings.ServiceBusBaseUrl)
                && !string.IsNullOrWhiteSpace(_settings.ServiceBusSasKeyName)
                && !string.IsNullOrWhiteSpace(_settings.ServiceBusSasKey);
            if (localConfigured)
                return new ResolvedServiceBusSettings(_settings.ServiceBusBaseUrl, _settings.ServiceBusSasKeyName, _settings.ServiceBusSasKey, "job settings");
            if (resolvedServiceBusSettings != null)
                return resolvedServiceBusSettings;
            return ResolvedServiceBusSettings.Empty("none");
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
                sessionSegment = "\"SessionId\":\"" + _settings.ServiceBusSessionId + "\",";

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
