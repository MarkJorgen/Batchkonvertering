using System;
using System.Collections.Generic;
using dk.gi.app.konto.kontoejerLuk.Application.Models;

namespace dk.gi.app.konto.kontoejerLuk.Application.Services
{
    public sealed class KontoejerLukPlanner
    {
        public IReadOnlyCollection<AccountOwnerClosure> PlanClosures(DeletedAccountRecord account, IReadOnlyCollection<AccountOwnerRecord> owners)
        {
            var result = new List<AccountOwnerClosure>();
            if (account == null || owners == null || owners.Count == 0)
                return result;

            if (!account.LastAccountingDateUtc.HasValue)
                return result;

            DateTime closeDate = account.LastAccountingDateUtc.Value.ToLocalTime().Date;
            foreach (var owner in owners)
            {
                if (owner == null || owner.OwnerId == Guid.Empty)
                    continue;
                if (owner.EndDateUtc.HasValue)
                    continue;

                result.Add(new AccountOwnerClosure(account.AccountId, account.AccountNumber, owner.OwnerId, closeDate));
            }

            return result;
        }
    }
}
