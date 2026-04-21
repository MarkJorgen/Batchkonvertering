using System;

namespace dk.gi.app.konto.kontoejerLuk.Application.Models
{
    public sealed class DeletedAccountRecord
    {
        public Guid AccountId { get; }
        public string AccountNumber { get; }
        public DateTime? LastAccountingDateUtc { get; }

        public DeletedAccountRecord(Guid accountId, string accountNumber, DateTime? lastAccountingDateUtc)
        {
            AccountId = accountId;
            AccountNumber = accountNumber ?? string.Empty;
            LastAccountingDateUtc = lastAccountingDateUtc;
        }
    }
}
