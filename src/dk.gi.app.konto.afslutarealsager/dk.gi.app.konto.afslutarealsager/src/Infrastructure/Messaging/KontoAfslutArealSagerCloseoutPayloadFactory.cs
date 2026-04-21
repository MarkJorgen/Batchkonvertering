using System;
using System.Text;
using dk.gi.app.konto.afslutarealsager.Application.Models;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Messaging
{
    public static class KontoAfslutArealSagerCloseoutPayloadFactory
    {
        public static string Create(KontoAfslutArealSagerCandidate candidate)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));

            var builder = new StringBuilder();
            builder.Append("{\"KeyValueList\":[");
            builder.Append("{\"Key\":\"Mode\",\"Value\":\"Incident\"},");
            builder.Append("{\"Key\":\"action\",\"Value\":\"luksagaktiviteter\"},");
            builder.Append("{\"Key\":\"sagsnr\",\"Value\":\"").Append(candidate.CaseNumber ?? string.Empty).Append("\"},");
            builder.Append("{\"Key\":\"beskrivelse\",\"Value\":\"Luk areal check\"},");
            builder.Append("{\"Key\":\"Kilde\",\"Value\":\"Batch:dk.gi.app.konto.afslutarealsager\"}");
            builder.Append("]}");
            return builder.ToString();
        }
    }
}
