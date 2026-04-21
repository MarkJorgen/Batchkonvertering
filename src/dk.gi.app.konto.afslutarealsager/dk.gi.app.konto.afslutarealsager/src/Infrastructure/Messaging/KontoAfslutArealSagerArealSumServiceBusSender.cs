using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using dk.gi.app.konto.afslutarealsager.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Messaging
{
    public sealed class KontoAfslutArealSagerArealSumServiceBusSender
    {
        private readonly KontoAfslutArealSagerSettings _settings;
        private readonly IJobLogger _logger;

        public KontoAfslutArealSagerArealSumServiceBusSender(KontoAfslutArealSagerSettings settings, IJobLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public bool Send(KontoAfslutArealSagerCandidate candidate, string areaId, ResolvedServiceBusSettings resolvedServiceBusSettings, int scheduleDelaySeconds)
        {
            var effective = GetEffectiveSettings(resolvedServiceBusSettings);
            if (!effective.IsConfigured)
            {
                _logger.Error("Service Bus settings mangler. AREALSUM2KONTO-job kan ikke publiceres. Kilde=" + effective.Source);
                return false;
            }

            string queueName = string.IsNullOrWhiteSpace(_settings.ServiceBusQueueName) ? "crmpluginjobs" : _settings.ServiceBusQueueName;
            string endpoint = effective.BaseUrl.TrimEnd('/') + "/" + queueName.Trim('/') + "/messages";
            string resourceUri = effective.BaseUrl.TrimEnd('/') + "/" + queueName.Trim('/');
            string payload = KontoAfslutArealSagerArealSumPayloadFactory.Create(candidate, areaId);
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
                    _logger.Info("Publicerede AREALSUM2KONTO-job for konto " + candidate.AccountNumber + " og areal " + areaId + " via " + effective.Source + ".");
                    return true;
                }

                string body = response.Content != null ? response.Content.ReadAsStringAsync().GetAwaiter().GetResult() : string.Empty;
                _logger.Error("Service Bus-publicering af AREALSUM2KONTO fejlede for konto " + candidate.AccountNumber + ". Status=" + (int)response.StatusCode + " Reason=" + response.ReasonPhrase + ". Kilde=" + effective.Source + ". Body=" + body);
                return false;
            }
        }

        private ResolvedServiceBusSettings GetEffectiveSettings(ResolvedServiceBusSettings resolvedServiceBusSettings)
        {
            bool localConfigured = !string.IsNullOrWhiteSpace(_settings.ServiceBusBaseUrl) && !string.IsNullOrWhiteSpace(_settings.ServiceBusSasKeyName) && !string.IsNullOrWhiteSpace(_settings.ServiceBusSasKey);
            if (localConfigured)
            {
                return new ResolvedServiceBusSettings(_settings.ServiceBusBaseUrl, _settings.ServiceBusSasKeyName, _settings.ServiceBusSasKey, "job settings");
            }
            return resolvedServiceBusSettings ?? ResolvedServiceBusSettings.Empty("none");
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
            return "{\"Label\":\"AREALSUM2KONTO\",\"MessageId\":\"" + messageId + "\"" + scheduled + "}";
        }

        private static string BuildSasToken(string resourceUri, string sasKeyName, string sasKey)
        {
            long expiry = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
            string encodedResourceUri = Uri.EscapeDataString(resourceUri);
            string toSign = encodedResourceUri + "\n" + expiry;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sasKey)))
            {
                string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(toSign)));
                return "SharedAccessSignature sr=" + encodedResourceUri + "&sig=" + Uri.EscapeDataString(signature) + "&se=" + expiry + "&skn=" + Uri.EscapeDataString(sasKeyName);
            }
        }
    }
}
