using System;
using System.Text;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Messaging
{
    internal static class RunoutRegistreringPayloadFactory
    {
        public static string Create(RunoutRegistreringCandidate candidate)
        {
            if (candidate == null)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            var builder = new StringBuilder();
            builder.Append("{\"KeyValueList\":[");
            builder.Append("{\"Key\":\"mode\",\"Value\":\"laan\"},");
            builder.Append("{\"Key\":\"action\",\"Value\":\"updudloebet\"},");
            builder.Append("{\"Key\":\"Info\",\"Value\":\"Oprettet fra lokal GI-fri RunoutRegistreringJobPublisher\"},");
            builder.Append("{\"Key\":\"sagsnr\",\"Value\":\"").Append(Escape(candidate.Sagsnr)).Append("\"},");
            builder.Append("{\"Key\":\"id\",\"Value\":\"").Append(candidate.Id.ToString("D").ToUpperInvariant()).Append("\"}");
            builder.Append("]}");
            return builder.ToString();
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }
    }
}
