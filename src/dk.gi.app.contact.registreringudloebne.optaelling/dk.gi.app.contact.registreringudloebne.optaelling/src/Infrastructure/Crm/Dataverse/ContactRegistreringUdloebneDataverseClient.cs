using System;
using System.Collections.Generic;
using System.Linq;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Abstractions;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Crm
{
    public sealed class ContactRegistreringUdloebneDataverseClient : IRunoutRegistreringScanClient
    {
        private const string RegistreringLogicalName = "ap_registrering";
        private const string TreklipLogicalName = "ap_treklip";
        private const string ConfigLogicalName = "config_configurationsetting";
        private const string ConfigName = "config_name";
        private const string ConfigValue = "config_ntextcolumn";
        private const string RegistreringSagsnr = "ap_sagsnr";
        private const string RegistreringTreklip = "ap_treklipid";
        private const string RegistreringStatus = "ap_status";
        private const string RegistreringTimeoutDato = "ap_timeoutdato";
        private const string RegistreringTimeoutDato2 = "ap_timeoutdato2";
        private const string TreklipStatus = "ap_status";
        private const string ConfigStatusCode = "statuscode";
        private const string ServiceBusConfigPrefix = "Azure.Service.Bus.Queue.Crm.Indbakke.";
        private const int StatusIgangvaerende = 1;
        private const int StatusUdlobetBortfalderOmToAar = 3;
        private const int StatusTreklipAfsluttet = 5;
        private const int StatusAktiv = 1;

        private readonly ServiceClient _serviceClient;
        private readonly IOrganizationService _service;
        private bool _disposed;

        public ContactRegistreringUdloebneDataverseClient(string connectionString)
        {
            _serviceClient = new ServiceClient(connectionString);
            if (_serviceClient == null)
                throw new InvalidOperationException("Failed to connect to Dataverse. ServiceClient instance was null.");
            if (!_serviceClient.IsReady)
                throw new InvalidOperationException("Failed to connect to Dataverse. LastError='" + (_serviceClient.LastError ?? string.Empty) + "'. LastException='" + (_serviceClient.LastException != null ? _serviceClient.LastException.ToString() : string.Empty) + "'.");
            _service = (IOrganizationService)_serviceClient;
        }

        public ContactRegistreringUdloebneExecutionSummary VerifyConnection()
            => ContactRegistreringUdloebneOperationResult.Success("CRM-forbindelse valideret. Der blev ikke udført workflow-kald i VERIFYCRM-mode.", "local dataverse sdk");

        public IReadOnlyCollection<RunoutRegistreringCandidate> FindCandidates(Guid? registreringId)
        {
            if (registreringId.HasValue)
            {
                Entity item = _service.Retrieve(RegistreringLogicalName, registreringId.Value, new ColumnSet("ap_registreringid", RegistreringSagsnr, RegistreringTreklip, RegistreringStatus, RegistreringTimeoutDato, RegistreringTimeoutDato2));
                if (item == null) return Array.Empty<RunoutRegistreringCandidate>();
                return new[] { ToCandidate(item) };
            }

            DateTime timestamp = DateTime.Now;
            var query = new QueryExpression(RegistreringLogicalName)
            {
                ColumnSet = new ColumnSet("ap_registreringid", RegistreringStatus, RegistreringTreklip, RegistreringSagsnr),
                Distinct = false
            };
            query.Criteria.FilterOperator = LogicalOperator.Or;

            var f1 = new FilterExpression(LogicalOperator.And);
            f1.AddCondition(RegistreringStatus, ConditionOperator.Equal, StatusIgangvaerende);
            f1.AddCondition(RegistreringTimeoutDato2, ConditionOperator.NotNull);
            f1.AddCondition(RegistreringTimeoutDato2, ConditionOperator.OnOrBefore, timestamp);
            query.Criteria.AddFilter(f1);

            var f2 = new FilterExpression(LogicalOperator.And);
            f2.AddCondition(RegistreringStatus, ConditionOperator.Equal, StatusUdlobetBortfalderOmToAar);
            f2.AddCondition(RegistreringTimeoutDato, ConditionOperator.NotNull);
            f2.AddCondition(RegistreringTimeoutDato, ConditionOperator.OnOrBefore, timestamp);
            query.Criteria.AddFilter(f2);

            return RetrieveAll(query).Select(ToCandidate).ToList();
        }

        public IReadOnlyCollection<Guid> GetClosedTreklipIds(IReadOnlyCollection<Guid> treklipIds)
        {
            if (treklipIds == null || treklipIds.Count == 0) return Array.Empty<Guid>();

            var query = new QueryExpression(TreklipLogicalName)
            {
                ColumnSet = new ColumnSet("ap_treklipid"),
                Distinct = false
            };
            query.Criteria.FilterOperator = LogicalOperator.And;
            query.Criteria.AddCondition(TreklipStatus, ConditionOperator.Equal, StatusTreklipAfsluttet);

            var idsFilter = new FilterExpression(LogicalOperator.Or);
            foreach (Guid id in treklipIds.Distinct()) idsFilter.AddCondition("ap_treklipid", ConditionOperator.Equal, id);
            query.Criteria.AddFilter(idsFilter);

            return RetrieveAll(query).Select(entity => entity.Id).Distinct().ToList();
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

            var values = RetrieveAll(query)
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

        private static RunoutRegistreringCandidate ToCandidate(Entity entity)
        {
            Guid? treklipId = entity.GetAttributeValue<EntityReference>(RegistreringTreklip)?.Id;
            string sagsnr = entity.GetAttributeValue<string>(RegistreringSagsnr) ?? string.Empty;
            return new RunoutRegistreringCandidate(entity.Id, sagsnr, treklipId);
        }

        private List<Entity> RetrieveAll(QueryExpression query)
        {
            var result = new List<Entity>();
            query.PageInfo = new PagingInfo { Count = 5000, PageNumber = 1 };
            while (true)
            {
                EntityCollection response = _service.RetrieveMultiple(query);
                result.AddRange(response.Entities);
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
