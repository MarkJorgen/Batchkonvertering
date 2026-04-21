using System;

namespace dk.gi.app.konto.kontoejerLuk.Application.Models
{
    public sealed class AccountOwnerClosure
    {
        public Guid AccountId { get; }
        public string AccountNumber { get; }
        public Guid OwnerId { get; }
        public DateTime CloseDateLocal { get; }

        public AccountOwnerClosure(Guid accountId, string accountNumber, Guid ownerId, DateTime closeDateLocal)
        {
            AccountId = accountId;
            AccountNumber = accountNumber ?? string.Empty;
            OwnerId = ownerId;
            CloseDateLocal = closeDateLocal;
        }
    }
}
