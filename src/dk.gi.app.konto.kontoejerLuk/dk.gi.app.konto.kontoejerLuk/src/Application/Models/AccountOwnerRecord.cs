using System;

namespace dk.gi.app.konto.kontoejerLuk.Application.Models
{
    public sealed class AccountOwnerRecord
    {
        public Guid OwnerId { get; }
        public DateTime? EndDateUtc { get; }

        public AccountOwnerRecord(Guid ownerId, DateTime? endDateUtc)
        {
            OwnerId = ownerId;
            EndDateUtc = endDateUtc;
        }
    }
}
