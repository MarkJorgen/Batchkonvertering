using System;

namespace dk.gi.app.konto.startarealtjek.Application.Models
{
    public sealed class KontoStartArealTjekCandidate
    {
        public Guid AccountId { get; }
        public string AccountNumber { get; }
        public KontoStartArealTjekPropertyType PropertyType { get; }
        public DateTime? LastArealCheckUtc { get; }

        public KontoStartArealTjekCandidate(Guid accountId, string accountNumber, KontoStartArealTjekPropertyType propertyType, DateTime? lastArealCheckUtc)
        {
            AccountId = accountId;
            AccountNumber = accountNumber ?? string.Empty;
            PropertyType = propertyType;
            LastArealCheckUtc = lastArealCheckUtc;
        }
    }
}
