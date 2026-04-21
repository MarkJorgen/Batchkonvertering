using System;
using System.Collections.Generic;
using System.Linq;
using dk.gi.app.contact.selskab.Application.Abstractions;
using dk.gi.app.contact.selskab.Application.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace dk.gi.app.contact.selskab.Infrastructure.Crm
{
    public sealed class ContactSelskabDataverseClient : IContactSelskabScanClient
    {
        private const string RealOwnerLogicalName = "ap_reelejer";
        private const string ContactLogicalName = "contact";
        private const string ConfigLogicalName = "config_configurationsetting";

        private const string RealOwnerEndDate = "ap_slutdato";
        private const string RealOwnerCompany = "ap_selskabid";
        private const string RealOwnerUltimateOwner = "ap_ultimativejerid";

        private const string ContactId = "contactid";
        private const string ContactKdk = "ap_kenddinkundedokumentationindhentet";
        private const string ContactBusinessId = "ap_virksomhedsid";

        private const string ConfigName = "config_name";
        private const string ConfigValue = "config_ntextcolumn";
        private const string ConfigStatusCode = "statuscode";
        private const string ServiceBusConfigPrefix = "Azure.Service.Bus.Queue.Crm.Indbakke.";
        private const int StatusAktiv = 1;

        private readonly ServiceClient _serviceClient;
        private readonly IOrganizationService _service;
        private readonly Action<string> _trace;
        private bool _disposed;

        public ContactSelskabDataverseClient(string connectionString, int timeOutMinutes, Action<string> trace = null)
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

        public ContactSelskabExecutionSummary VerifyConnection()
            => new ContactSelskabExecutionSummary(true, 0, 0, 0, "CRM-forbindelse valideret. Der blev ikke udført workflow-kald i VERIFYCRM-mode.", "local dataverse sdk");

        public IReadOnlyCollection<ContactSelskabOwnerObservation> GetOwnerObservations(Guid? companyContactId)
        {
            var query = new QueryExpression(RealOwnerLogicalName)
            {
                ColumnSet = new ColumnSet(false),
                Distinct = false,
                NoLock = true,
                PageInfo = new PagingInfo { PageNumber = 1, Count = 500 }
            };

            query.Criteria.AddCondition(RealOwnerEndDate, ConditionOperator.Null);

            var companyLink = query.AddLink(ContactLogicalName, RealOwnerCompany, ContactId);
            companyLink.EntityAlias = "selskab";
            companyLink.Columns.AddColumns(ContactId, ContactKdk, ContactBusinessId);
            companyLink.LinkCriteria.AddCondition(ContactBusinessId, ConditionOperator.NotNull);
            companyLink.LinkCriteria.AddCondition(ContactKdk, ConditionOperator.NotEqual, true);

            if (companyContactId.HasValue)
            {
                companyLink.LinkCriteria.AddCondition(ContactId, ConditionOperator.Equal, companyContactId.Value);
            }

            var ownerLink = query.AddLink(ContactLogicalName, RealOwnerUltimateOwner, ContactId);
            ownerLink.EntityAlias = "owner";
            ownerLink.Columns.AddColumns(ContactKdk);

            return RetrieveAll(query, "contact-selskab-owner-observations")
                .Select(ToObservation)
                .Where(x => x != null)
                .ToList();
        }

        public ResolvedServiceBusSettings ResolveServiceBusSettings()
        {
            var query = new QueryExpression(ConfigLogicalName)
            {
                ColumnSet = new ColumnSet(ConfigName, ConfigValue),
                Distinct = false,
                NoLock = true,
                PageInfo = new PagingInfo { PageNumber = 1, Count = 100 }
            };
            query.Criteria.AddCondition(ConfigStatusCode, ConditionOperator.Equal, StatusAktiv);
            query.Criteria.AddCondition(ConfigName, ConditionOperator.Like, ServiceBusConfigPrefix + "%");
            query.AddOrder(ConfigName, OrderType.Ascending);

            var values = RetrieveAll(query, "crm-servicebus-settings")
                .Where(x => x.Attributes.Contains(ConfigName) && x.Attributes.Contains(ConfigValue))
                .ToDictionary(
                    x => x.GetAttributeValue<string>(ConfigName) ?? string.Empty,
                    x => x.GetAttributeValue<string>(ConfigValue) ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);

            string serviceBusName;
            string sasKeyName;
            string sasKey;

            values.TryGetValue(ServiceBusConfigPrefix + "ServiceBusName", out serviceBusName);
            values.TryGetValue(ServiceBusConfigPrefix + "SharedAccessKeyName", out sasKeyName);
            values.TryGetValue(ServiceBusConfigPrefix + "SharedAccessKey", out sasKey);

            if (string.IsNullOrWhiteSpace(serviceBusName)
                || string.IsNullOrWhiteSpace(sasKeyName)
                || string.IsNullOrWhiteSpace(sasKey))
            {
                return ResolvedServiceBusSettings.Empty("crm config_configurationsetting");
            }

            return new ResolvedServiceBusSettings(
                "https://" + serviceBusName.Trim().TrimEnd('/') + ".servicebus.windows.net",
                sasKeyName.Trim(),
                sasKey.Trim(),
                "crm config_configurationsetting");
        }

        private ContactSelskabOwnerObservation ToObservation(Entity entity)
        {
            Guid companyId = ReadAliasedGuid(entity, "selskab." + ContactId);
            if (companyId == Guid.Empty)
            {
                return null;
            }

            string cvrNumber = ReadAliasedString(entity, "selskab." + ContactBusinessId);
            bool ownerHasKdk = ReadAliasedBool(entity, "owner." + ContactKdk);

            return new ContactSelskabOwnerObservation(companyId, cvrNumber, ownerHasKdk);
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

                _trace?.Invoke(
                    "Query '" + label + "' side " + page +
                    " hentet på " + seconds.ToString("0.0") +
                    " sek. Batch=" + response.Entities.Count +
                    ", total=" + result.Count +
                    ", moreRecords=" + (response.MoreRecords ? "Ja" : "Nej"));

                if (!response.MoreRecords)
                {
                    break;
                }

                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = response.PagingCookie;
            }

            return result;
        }

        private static Guid ReadAliasedGuid(Entity entity, string key)
        {
            var value = ReadAliasedValue(entity, key);
            if (value is Guid guid) return guid;
            if (value is EntityReference reference) return reference.Id;
            return Guid.Empty;
        }

        private static string ReadAliasedString(Entity entity, string key)
        {
            var value = ReadAliasedValue(entity, key);
            return value as string ?? string.Empty;
        }

        private static bool ReadAliasedBool(Entity entity, string key)
        {
            var value = ReadAliasedValue(entity, key);
            if (value is bool direct) return direct;
            if (value is OptionSetValue option) return option.Value != 0;
            return false;
        }

        private static object ReadAliasedValue(Entity entity, string key)
        {
            if (entity == null || string.IsNullOrWhiteSpace(key) || !entity.Attributes.Contains(key))
            {
                return null;
            }

            var aliased = entity.Attributes[key] as AliasedValue;
            return aliased?.Value;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _serviceClient.Dispose();
            _disposed = true;
        }
    }
}
