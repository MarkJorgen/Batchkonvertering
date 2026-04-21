using System;

namespace dk.gi.app.konto.startarealtjek.Application.Models
{
    public sealed class KontoStartArealTjekAccount
    {
        public Guid AccountId { get; }
        public string AccountNumber { get; }
        public KontoStartArealTjekPropertyType PropertyType { get; }
        public string InUseYearRaw { get; }
        public int? InUseYear { get; }
        public bool? ExistingIsSubject { get; }
        public DateTime? LastArealCheckUtc { get; }

        public KontoStartArealTjekAccount(
            Guid accountId,
            string accountNumber,
            KontoStartArealTjekPropertyType propertyType,
            string inUseYearRaw,
            int? inUseYear,
            bool? existingIsSubject,
            DateTime? lastArealCheckUtc)
        {
            AccountId = accountId;
            AccountNumber = accountNumber ?? string.Empty;
            PropertyType = propertyType;
            InUseYearRaw = inUseYearRaw ?? string.Empty;
            InUseYear = inUseYear;
            ExistingIsSubject = existingIsSubject;
            LastArealCheckUtc = lastArealCheckUtc;
        }
    }
}
