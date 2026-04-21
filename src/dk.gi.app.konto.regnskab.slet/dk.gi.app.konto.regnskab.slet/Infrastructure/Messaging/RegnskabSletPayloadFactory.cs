using System;
using System.Text;
using dk.gi.app.konto.regnskab.slet.Application.Models;

namespace dk.gi.app.konto.regnskab.slet.Infrastructure.Messaging
{
    public static class RegnskabSletPayloadFactory
    {
        public static string Create(KontoCandidate candidate)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));

            var builder = new StringBuilder();
            builder.Append("{\"KeyValueList\":[");
            builder.Append("{\"Key\":\"Mode\",\"Value\":\"Regnskab\"},");
            builder.Append("{\"Key\":\"action\",\"Value\":\"sletregnskaber\"},");
            builder.Append("{\"Key\":\"ap_konto\",\"Value\":\"")
                .Append(candidate.AccountId.ToString("D").ToUpperInvariant())
                .Append("\"}");
            builder.Append("]}");
            return builder.ToString();
        }
    }
}
