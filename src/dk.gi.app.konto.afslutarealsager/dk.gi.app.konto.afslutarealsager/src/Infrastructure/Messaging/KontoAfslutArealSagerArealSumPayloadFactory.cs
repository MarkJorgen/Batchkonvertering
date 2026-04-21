using System;
using System.Text;
using dk.gi.app.konto.afslutarealsager.Application.Models;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Messaging
{
    public static class KontoAfslutArealSagerArealSumPayloadFactory
    {
        public static string Create(KontoAfslutArealSagerCandidate candidate, string areaId)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));
            if (string.IsNullOrWhiteSpace(areaId)) throw new ArgumentException("areaId mangler værdi.", nameof(areaId));

            var builder = new StringBuilder();
            builder.Append("{\"KeyValueList\":[");
            builder.Append("{\"Key\":\"action\",\"Value\":\"UPDATE\"},");
            builder.Append("{\"Key\":\"id\",\"Value\":\"").Append(areaId).Append("\"},");
            builder.Append("{\"Key\":\"logicalname\",\"Value\":\"ap_areal\"},");
            builder.Append("{\"Key\":\"kontonr\",\"Value\":\"").Append(candidate.AccountNumber ?? string.Empty).Append("\"}");
            builder.Append("]}");
            return builder.ToString();
        }
    }
}
