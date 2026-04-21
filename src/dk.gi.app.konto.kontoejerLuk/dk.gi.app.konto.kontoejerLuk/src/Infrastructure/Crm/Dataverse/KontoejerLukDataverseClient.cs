using System;
using System.Collections.Generic;
using System.Linq;
using dk.gi.app.konto.kontoejerLuk.Application.Abstractions;
using dk.gi.app.konto.kontoejerLuk.Application.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace dk.gi.app.konto.kontoejerLuk.Infrastructure.Crm.Dataverse
{
    public sealed class KontoejerLukDataverseClient : IKontoejerLukScanClient
    {
        private const string AccountLogicalName = "ap_konto";
        private const string OwnerLogicalName = "ap_kontoejer";

        private const string AccountId = "ap_kontoid";
        private const string AccountNumber = "ap_kontonr";
        private const string AccountLastAccountingDate = "ap_sidsteregnskabsdato";
        private const string AccountStatusFraMapper = "ap_statusframapper";

        private const string OwnerId = "ap_kontoejerid";
        private const string OwnerAccountLookup = "ap_kontoid";
        private const string OwnerEndDate = "ap_slutdato";

        private readonly ServiceClient _serviceClient;
        private readonly IOrganizationService _service;
        private readonly Action<string> _trace;
        private bool _disposed;

        public KontoejerLukDataverseClient(string connectionString, int timeOutMinutes, Action<string> trace = null)
        {
            _trace = trace;
            ServiceClient.MaxConnectionTimeout = TimeSpan.FromMinutes(Math.Max(1, timeOutMinutes));
            _serviceClient = new ServiceClient(connectionString);
            _service = _serviceClient;

            if (!_serviceClient.IsReady)
                throw new InvalidOperationException("Dataverse klient kunne ikke oprettes: " + _serviceClient.LastError);
        }

        public KontoejerLukExecutionSummary VerifyConnection()
        {
            var query = new QueryExpression(AccountLogicalName)
            {
                ColumnSet = new ColumnSet(AccountId),
                TopCount = 1,
                NoLock = true
            };

            _service.RetrieveMultiple(query);
            return new KontoejerLukExecutionSummary(true, 0, 0, 0, "Dataverse-forbindelse verificeret.", "local dataverse sdk");
        }

        public IReadOnlyCollection<DeletedAccountRecord> GetDeletedAccounts()
        {
            var query = new QueryExpression(AccountLogicalName)
            {
                ColumnSet = new ColumnSet(AccountId, AccountNumber, AccountLastAccountingDate),
                Distinct = true,
                NoLock = true,
                PageInfo = new PagingInfo { PageNumber = 1, Count = 500 }
            };
            query.Criteria.AddCondition(AccountStatusFraMapper, ConditionOperator.Equal, 2);

            return RetrieveAll(query, "kontoejerLuk-deleted-accounts")
                .Select(entity => new DeletedAccountRecord(
                    entity.Id,
                    entity.GetAttributeValue<string>(AccountNumber) ?? string.Empty,
                    entity.Attributes.Contains(AccountLastAccountingDate)
                        ? (DateTime?)entity.GetAttributeValue<DateTime>(AccountLastAccountingDate)
                        : null))
                .ToList();
        }

        public IReadOnlyCollection<AccountOwnerRecord> GetOpenOwners(Guid accountId)
        {
            var query = new QueryExpression(OwnerLogicalName)
            {
                ColumnSet = new ColumnSet(OwnerId, OwnerEndDate),
                Distinct = false,
                NoLock = true,
                PageInfo = new PagingInfo { PageNumber = 1, Count = 100 }
            };
            query.Criteria.AddCondition(OwnerAccountLookup, ConditionOperator.Equal, accountId);
            query.Criteria.AddCondition(OwnerEndDate, ConditionOperator.Null);

            return RetrieveAll(query, "kontoejerLuk-open-owners")
                .Select(entity => new AccountOwnerRecord(
                    entity.Id,
                    entity.Attributes.Contains(OwnerEndDate)
                        ? (DateTime?)entity.GetAttributeValue<DateTime>(OwnerEndDate)
                        : null))
                .ToList();
        }

        public void ApplyOwnerClosures(IReadOnlyCollection<AccountOwnerClosure> closures)
        {
            if (closures == null || closures.Count == 0)
            {
                _trace?.Invoke("Ingen kontoejere skulle opdateres.");
                return;
            }

            int updated = 0;
            foreach (var closure in closures)
            {
                var entity = new Entity(OwnerLogicalName) { Id = closure.OwnerId };
                entity[OwnerEndDate] = closure.CloseDateLocal;
                _service.Update(entity);
                updated++;
                _trace?.Invoke("Kontoejer opdateret. Konto=" + closure.AccountNumber + ", Kontoejer=" + closure.OwnerId + ", Slutdato=" + closure.CloseDateLocal.ToString("yyyy-MM-dd") + ".");
            }

            _trace?.Invoke("Opdaterede ap_slutdato på " + updated + " kontoejer(e).");
        }

        private List<Entity> RetrieveAll(QueryExpression query, string operationName)
        {
            var result = new List<Entity>();
            string pagingCookie = null;
            int pageNumber = 1;

            while (true)
            {
                query.PageInfo = query.PageInfo ?? new PagingInfo();
                query.PageInfo.PageNumber = pageNumber;
                query.PageInfo.PagingCookie = pagingCookie;
                EntityCollection response = _service.RetrieveMultiple(query);
                if (response?.Entities != null && response.Entities.Count > 0)
                    result.AddRange(response.Entities);

                _trace?.Invoke(operationName + ": side=" + pageNumber + ", hentet=" + (response?.Entities?.Count ?? 0) + ".");

                if (response == null || !response.MoreRecords)
                    break;

                pageNumber++;
                pagingCookie = response.PagingCookie;
            }

            return result;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _serviceClient.Dispose();
        }
    }
}
