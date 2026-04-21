using System;
using System.Text;
using dk.gi.app.ejendom.tjekejerskifte.Application.Models;

namespace dk.gi.app.ejendom.tjekejerskifte.Infrastructure.Messaging
{
    public static class EjendomTjekEjerskiftePayloadFactory
    {
        public static string Create(EjendomTjekEjerskifteCandidate candidate)
        {
            if (candidate == null)
                throw new ArgumentNullException(nameof(candidate));

            var builder = new StringBuilder();
            builder.Append("{\"KeyValueList\":[");
            builder.Append("{\"Key\":\"ap_ejendom\",\"Value\":\"").Append(candidate.PropertyId.ToString("D").ToUpperInvariant()).Append("\"},");
            builder.Append("{\"Key\":\"ap_kommunenummer\",\"Value\":\"").Append(Escape(candidate.KommuneNummer)).Append("\"},");
            builder.Append("{\"Key\":\"ap_ejendomsnummer\",\"Value\":\"").Append(Escape(candidate.EjendomsNummer)).Append("\"},");
            builder.Append("{\"Key\":\"ap_bbrnummer\",\"Value\":\"").Append(Escape(candidate.BbrNummer)).Append("\"},");
            builder.Append("{\"Key\":\"ap_bfenummer\",\"Value\":\"").Append(Escape(candidate.BfeNummer)).Append("\"},");
            builder.Append("{\"Key\":\"ap_bfenummermoderejendom\",\"Value\":\"").Append(Escape(candidate.BfeNummerModerEjendom)).Append("\"},");
            builder.Append("{\"Key\":\"ap_sidsteskoededato\",\"Value\":\"").Append(candidate.SidsteSkoedeDatoUtc.HasValue ? candidate.SidsteSkoedeDatoUtc.Value.ToLocalTime().ToString("yyyyMMdd") : "0").Append("\"}");
            builder.Append("]}");
            return builder.ToString();
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
