using System;
using System.Text;
using dk.gi.app.contact.selskab.Application.Models;

namespace dk.gi.app.contact.selskab.Infrastructure.Messaging
{
    public static class ContactSelskabPayloadFactory
    {
        public static string Create(ContactSelskabCandidate candidate)
        {
            if (candidate == null)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            var builder = new StringBuilder();
            builder.Append("{\"KeyValueList\":[");
            builder.Append("{\"Key\":\"Mode\",\"Value\":\"Contact\"},");
            builder.Append("{\"Key\":\"action\",\"Value\":\"opdaterkdk\"},");
            builder.Append("{\"Key\":\"Info\",\"Value\":\"Oprettet fra lokal GI-fri ContactSelskabJobPublisher\"},");
            builder.Append("{\"Key\":\"contactid\",\"Value\":\"").Append(candidate.CompanyId.ToString("D").ToUpperInvariant()).Append("\"}");
            builder.Append("]}");
            return builder.ToString();
        }
    }
}
