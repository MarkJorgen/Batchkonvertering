using System;
using System.Collections.Generic;
using System.Linq;
using dk.gi.app.contact.lassox.ophoer.Application.Abstractions;
using dk.gi.app.contact.lassox.ophoer.Application.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace dk.gi.app.contact.lassox.ophoer.Infrastructure.Crm
{
    public sealed class LassoXOphoerDataverseClient : ILassoXOphoerScanClient
    {
        private const string ContactLogicalName = "contact";
        private const string AccountOwnerLogicalName = "ap_kontoejer";
        private const string RealOwnerLogicalName = "ap_reelejer";
        private const string ContactIdAttribute = "contactid";
        private const string FullNameAttribute = "fullname";
        private const string LassoSubscriptionAttribute = "ap_lassoabonnement";
        private const string AccountOwnerContactAttribute = "ap_kontaktpersonid";
        private const string AccountOwnerEndDateAttribute = "ap_slutdato";
        private const string RealOwnerPrimaryIdAttribute = "ap_reelejerid";
        private const string RealOwnerCompanyAttribute = "ap_selskabid";
        private const string RealOwnerEndDateAttribute = "ap_slutdato";
        private const string StateCodeAttribute = "statecode";
        private const int LassoSubscribedValue = 1;
        private const int LassoUnsubscribedValue = 0;
        private const int ActiveStateValue = 0;
        private const int InactiveStateValue = 1;

        private readonly ServiceClient _serviceClient;
        private readonly IOrganizationService _service;
        private readonly Action<string> _trace;
        private bool _disposed;

        public LassoXOphoerDataverseClient(string connectionString, int timeOutMinutes, Action<string> trace = null)
        {
            _trace = trace;
            ServiceClient.MaxConnectionTimeout = TimeSpan.FromMinutes(Math.Max(1, timeOutMinutes));

            _serviceClient = new ServiceClient(connectionString);
            if (_serviceClient == null)
                throw new InvalidOperationException("Failed to connect to Dataverse. ServiceClient instance was null.");
            if (!_serviceClient.IsReady)
                throw new InvalidOperationException("Failed to connect to Dataverse. LastError='" + (_serviceClient.LastError ?? string.Empty) + "'. LastException='" + (_serviceClient.LastException != null ? _serviceClient.LastException.ToString() : string.Empty) + "'.");

            _service = (IOrganizationService)_serviceClient;
        }

        public LassoXOphoerExecutionSummary VerifyConnection()
            => new LassoXOphoerExecutionSummary(true, 0, 0, 0, "CRM-forbindelse valideret. Der blev ikke udført workflow-kald i VERIFYCRM-mode.", "local dataverse sdk");

        public IReadOnlyCollection<LassoXContactCandidate> GetContactsWithActiveSubscription(Guid? contactId)
        {
            var query = new QueryExpression(ContactLogicalName)
            {
                ColumnSet = new ColumnSet(ContactIdAttribute, FullNameAttribute, LassoSubscriptionAttribute),
                Distinct = false,
                NoLock = true,
                PageInfo = new PagingInfo { PageNumber = 1, Count = 500 }
            };

            query.Criteria.AddCondition(LassoSubscriptionAttribute, ConditionOperator.Equal, LassoSubscribedValue);
            if (contactId.HasValue)
            {
                query.Criteria.AddCondition(ContactIdAttribute, ConditionOperator.Equal, contactId.Value);
            }

            return RetrieveAll(query, "contacts-active-lassox")
                .Select(entity => new LassoXContactCandidate(
                    entity.Id,
                    entity.GetAttributeValue<string>(FullNameAttribute) ?? string.Empty))
                .ToList();
        }

        public IReadOnlyCollection<Guid> GetOpenAccountOwnerContactIds()
        {
            var query = new QueryExpression(AccountOwnerLogicalName)
            {
                ColumnSet = new ColumnSet(AccountOwnerContactAttribute),
                Distinct = true,
                NoLock = true,
                PageInfo = new PagingInfo { PageNumber = 1, Count = 1000 }
            };

            query.Criteria.AddCondition(AccountOwnerEndDateAttribute, ConditionOperator.Null);
            query.Criteria.AddCondition(StateCodeAttribute, ConditionOperator.NotEqual, InactiveStateValue);

            return RetrieveAll(query, "open-account-owners")
                .Select(entity => entity.GetAttributeValue<EntityReference>(AccountOwnerContactAttribute))
                .Where(reference => reference != null)
                .Select(reference => reference.Id)
                .Distinct()
                .ToList();
        }

        public IReadOnlyCollection<Guid> GetRealOwnerContactIds()
        {
            var query = new QueryExpression(RealOwnerLogicalName)
            {
                ColumnSet = new ColumnSet(RealOwnerPrimaryIdAttribute),
                Distinct = true,
                NoLock = true,
                PageInfo = new PagingInfo { PageNumber = 1, Count = 500 }
            };

            query.Criteria.AddCondition(StateCodeAttribute, ConditionOperator.Equal, ActiveStateValue);

            var endDateFilter = new FilterExpression(LogicalOperator.Or);
            endDateFilter.AddCondition(RealOwnerEndDateAttribute, ConditionOperator.Null);
            endDateFilter.AddCondition(RealOwnerEndDateAttribute, ConditionOperator.OnOrAfter, DateTime.Now);
            query.Criteria.AddFilter(endDateFilter);

            var result = new HashSet<Guid>();
            foreach (var entity in RetrieveAll(query, "real-owners"))
            {
                // Legacy-sporet sammenligner ap_reelejerid direkte mod kontakt-id.
                // Den adfærd bevares her for at matche den dokumenterede legacy-logik.
                if (entity.Id != Guid.Empty)
                {
                    result.Add(entity.Id);
                }
            }

            return result.ToList();
        }

        public int UnsubscribeContacts(IReadOnlyCollection<Guid> contactIds)
        {
            if (contactIds == null || contactIds.Count == 0)
            {
                return 0;
            }

            int updated = 0;
            foreach (Guid contactId in contactIds.Distinct())
            {
                var update = new Entity(ContactLogicalName, contactId);
                update[LassoSubscriptionAttribute] = new OptionSetValue(LassoUnsubscribedValue);
                _service.Update(update);
                updated++;

                if (_trace != null && updated % 100 == 0)
                {
                    _trace("Write-progress: opdateret " + updated + " af " + contactIds.Count + " kontakter.");
                }
            }

            return updated;
        }

        private List<Entity> RetrieveAll(QueryExpression query, string label)
        {
            var result = new List<Entity>();
            int page = 0;

            while (true)
            {
                page++;
                DateTime started = DateTime.UtcNow;
                EntityCollection response = _service.RetrieveMultiple(query);
                double seconds = (DateTime.UtcNow - started).TotalSeconds;

                result.AddRange(response.Entities);

                if (_trace != null)
                {
                    _trace(
                        "Query '" + label + "' side " + page +
                        " hentet på " + seconds.ToString("0.0") +
                        " sek. Batch=" + response.Entities.Count +
                        ", total=" + result.Count +
                        ", moreRecords=" + (response.MoreRecords ? "Ja" : "Nej"));
                }

                if (!response.MoreRecords) break;
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = response.PagingCookie;
            }
            return result;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _serviceClient.Dispose();
            _disposed = true;
        }
    }
}
