using System;
using System.Collections.Generic;
using dk.gi.app.konto.kontoejerLuk.Application.Models;

namespace dk.gi.app.konto.kontoejerLuk.Application.Abstractions
{
    public interface IKontoejerLukScanClient : IDisposable
    {
        KontoejerLukExecutionSummary VerifyConnection();
        IReadOnlyCollection<DeletedAccountRecord> GetDeletedAccounts();
        IReadOnlyCollection<AccountOwnerRecord> GetOpenOwners(Guid accountId);
        void ApplyOwnerClosures(IReadOnlyCollection<AccountOwnerClosure> closures);
    }
}
