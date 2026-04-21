using System;
using System.Text;
using dk.gi.app.konto.startarealtjek.Application.Models;

namespace dk.gi.app.konto.startarealtjek.Infrastructure.Messaging
{
    public static class KontoStartArealTjekPayloadFactory
    {
        public static string Create(KontoStartArealTjekCandidate candidate)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));

            var builder = new StringBuilder();
            builder.Append("{\"KeyValueList\":[");
            builder.Append("{\"Key\":\"action\",\"Value\":\"AREALCHECKSAG\"},");
            builder.Append("{\"Key\":\"Kilde\",\"Value\":\"Batch:dk.gi.app.konto.startarealtjek\"},");
            builder.Append("{\"Key\":\"id\",\"Value\":\"").Append(candidate.AccountId.ToString("D").ToUpperInvariant()).Append("\"},");
            builder.Append("{\"Key\":\"Kontonr\",\"Value\":\"").Append(candidate.AccountNumber).Append("\"}");
            builder.Append("]}");
            return builder.ToString();
        }
    }
}
