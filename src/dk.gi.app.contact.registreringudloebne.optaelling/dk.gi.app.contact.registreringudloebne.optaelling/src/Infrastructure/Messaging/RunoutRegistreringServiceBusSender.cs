using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Messaging
{
    public class RunoutRegistreringServiceBusSender
    {
        private readonly ContactRegistreringUdloebneOptaellingSettings _settings;
        private readonly IJobLogger _logger;

        public RunoutRegistreringServiceBusSender(ContactRegistreringUdloebneOptaellingSettings settings, IJobLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public virtual bool Send(RunoutRegistreringCandidate candidate, ResolvedServiceBusSettings resolvedServiceBusSettings)
        {
            var effective = GetEffectiveSettings(resolvedServiceBusSettings);
            var missingMessageSettings = GetMissingMessageSettings();

            if (!effective.IsConfigured || missingMessageSettings.Count > 0)
            {
                string missing = !effective.IsConfigured
                    ? "ServiceBusBaseUrl/ServiceBusSasKeyName/ServiceBusSasKey"
                    : string.Empty;

                if (missingMessageSettings.Count > 0)
                {
                    if (missing.Length > 0)
                    {
                        missing += ", ";
                    }

                    missing += string.Join(", ", missingMessageSettings);
                }

                _logger.Error("Service Bus settings mangler. Job kan ikke publiceres. Mangler=" + missing + ". Kilde=" + effective.Source);
                return false;
            }

            string endpoint = effective.BaseUrl.TrimEnd('/') + "/" + _settings.ServiceBusQueueName.Trim('/') + "/messages";
            string resourceUri = effective.BaseUrl.TrimEnd('/') + "/" + _settings.ServiceBusQueueName.Trim('/');
            string payload = RunoutRegistreringPayloadFactory.Create(candidate);
            string token = BuildSasToken(resourceUri, effective.SasKeyName, effective.SasKey);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                request.Headers.TryAddWithoutValidation("Authorization", token);
                request.Headers.TryAddWithoutValidation("BrokerProperties", "{\"Label\":\"" + _settings.ServiceBusLabel + "\",\"SessionId\":\"" + _settings.ServiceBusSessionId + "\"}");
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.SendAsync(request).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    _logger.Info("Publicerede Service Bus-job for registrering " + candidate.Id + " via " + effective.Source + ".");
                    return true;
                }

                string body = response.Content != null
                    ? response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                    : string.Empty;

                _logger.Error("Service Bus-publicering fejlede for registrering " + candidate.Id + ". Status=" + (int)response.StatusCode + " Reason=" + response.ReasonPhrase + ". Kilde=" + effective.Source + ". Body=" + body);
                return false;
            }
        }

        private ResolvedServiceBusSettings GetEffectiveSettings(ResolvedServiceBusSettings resolvedServiceBusSettings)
        {
            bool localConfigured = !string.IsNullOrWhiteSpace(_settings.ServiceBusBaseUrl)
                && !string.IsNullOrWhiteSpace(_settings.ServiceBusSasKeyName)
                && !string.IsNullOrWhiteSpace(_settings.ServiceBusSasKey);

            if (localConfigured)
            {
                return new ResolvedServiceBusSettings(
                    _settings.ServiceBusBaseUrl,
                    _settings.ServiceBusSasKeyName,
                    _settings.ServiceBusSasKey,
                    "job settings");
            }

            if (resolvedServiceBusSettings != null)
            {
                return resolvedServiceBusSettings;
            }

            return ResolvedServiceBusSettings.Empty("none");
        }

        private List<string> GetMissingMessageSettings()
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(_settings.ServiceBusQueueName)) missing.Add("ServiceBusQueueName");
            if (string.IsNullOrWhiteSpace(_settings.ServiceBusLabel)) missing.Add("ServiceBusLabel");
            if (string.IsNullOrWhiteSpace(_settings.ServiceBusSessionId)) missing.Add("ServiceBusSessionId");
            return missing;
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
