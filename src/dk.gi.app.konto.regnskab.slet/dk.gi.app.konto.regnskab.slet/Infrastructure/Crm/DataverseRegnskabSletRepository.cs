using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.regnskab.slet.Application.Contracts;
using dk.gi.app.konto.regnskab.slet.Application.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace dk.gi.app.konto.regnskab.slet.Infrastructure.Crm
{
    public sealed class DataverseRegnskabSletRepository : IRegnskabSletRepository
    {
        private const string AccountLogicalName = "ap_konto";
        private const string AccountId = "ap_kontoid";
        private const string AccountNumber = "ap_kontonr";
        private const string AccountStatusFromMapper = "ap_statusframapper";
        private const int AccountDeletedStatus = 2;
        private const string RegnskabLogicalName = "ap_regnskab";
        private const string RegnskabAccountId = "ap_kontoid";
        private const string RegnskabReasonId = "ap_regnskabrsagid";
        private const string RegnskabPeriodEnd = "ap_periodeslut";
        private const string RegnskabReasonLogicalName = "ap_regnskabsrsager";
        private const string RegnskabReasonCode = "ap_kode";
        private const string KontoSystemLogicalName = "ap_kontosystem";
        private const string KontoSystemId = "ap_kontosystemid";
        private const string KontoSystemExpiryDate = "ap_foraeldelsesfrist";
        private const string ConfigLogicalName = "config_configurationsetting";
        private const string ConfigName = "config_name";
        private const string ConfigValue = "config_ntextcolumn";
        private const string BatchCountConfig = "app.konto.regnskab.konti.sletning.koersel";
        private const string ServiceBusConfigPrefix = "Azure.Service.Bus.Queue.Crm.Indbakke.";

        private readonly DataverseConnectionFactory _connectionFactory;
        private readonly RegnskabSletSettings _settings;
        private readonly ILogger _logger;

        public DataverseRegnskabSletRepository(DataverseConnectionFactory connectionFactory, RegnskabSletSettings settings, ILogger logger)
        {
            _connectionFactory = connectionFactory;
            _settings = settings;
            _logger = logger;
        }

        public Task<IReadOnlyCollection<KontoCandidate>> GetCandidatesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var expiry = GetForaeldelsesfristUtc();
            var candidates = new List<KontoCandidate>();

            using (var client = _connectionFactory.CreateClient())
            {
                if (!client.IsReady) throw new InvalidOperationException(client.LastError ?? "Dataverse-klienten er ikke klar.");

                var query = new QueryExpression(AccountLogicalName)
                {
                    Distinct = true,
                    ColumnSet = new ColumnSet(AccountId, AccountNumber),
                    PageInfo = new PagingInfo { Count = 500, PageNumber = 1 }
                };
                query.Criteria.AddCondition(AccountStatusFromMapper, ConditionOperator.NotEqual, AccountDeletedStatus);

                var regnskabLink = query.AddLink(RegnskabLogicalName, AccountId, RegnskabAccountId);
                regnskabLink.LinkCriteria.AddCondition(RegnskabPeriodEnd, ConditionOperator.LessEqual, expiry);

                var reasonLink = regnskabLink.AddLink(RegnskabReasonLogicalName, RegnskabReasonId, "ap_regnskabsrsagerid");
                reasonLink.LinkCriteria.AddCondition(RegnskabReasonCode, ConditionOperator.NotIn, new object[] { "01" });

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var page = client.RetrieveMultiple(query);
                    foreach (var entity in page.Entities)
                    {
                        candidates.Add(new KontoCandidate
                        {
                            AccountId = entity.Id,
                            AccountNumber = entity.GetAttributeValue<string>(AccountNumber) ?? string.Empty,
                        });
                    }
                    if (!page.MoreRecords) break;
                    query.PageInfo.PageNumber += 1;
                    query.PageInfo.PagingCookie = page.PagingCookie;
                }
            }

            int selected = GetBatchCount();
            var limited = candidates.Take(Math.Min(selected, candidates.Count)).ToList();
            _logger.LogInformation("Fandt {Total} konti med forældede regnskaber. Udvalgt til denne kørsel: {Selected}.", candidates.Count, limited.Count);
            return Task.FromResult((IReadOnlyCollection<KontoCandidate>)limited);
        }

        public Task<ResolvedServiceBusSettings> ResolveServiceBusSettingsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var values = RetrieveConfigurationValues(ServiceBusConfigPrefix);
            string serviceBusName;
            string sasKeyName;
            string sasKey;
            values.TryGetValue(ServiceBusConfigPrefix + "ServiceBusName", out serviceBusName);
            values.TryGetValue(ServiceBusConfigPrefix + "SharedAccessKeyName", out sasKeyName);
            values.TryGetValue(ServiceBusConfigPrefix + "SharedAccessKey", out sasKey);
            if (string.IsNullOrWhiteSpace(serviceBusName) || string.IsNullOrWhiteSpace(sasKeyName) || string.IsNullOrWhiteSpace(sasKey))
            {
                return Task.FromResult(ResolvedServiceBusSettings.Empty("crm config_configurationsetting"));
            }
            return Task.FromResult(new ResolvedServiceBusSettings(
                "https://" + serviceBusName.Trim().TrimEnd('/') + ".servicebus.windows.net",
                sasKeyName.Trim(),
                sasKey.Trim(),
                "crm config_configurationsetting"));
        }

        private DateTime GetForaeldelsesfristUtc()
        {
            using (var client = _connectionFactory.CreateClient())
            {
                if (!client.IsReady) throw new InvalidOperationException(client.LastError ?? "Dataverse-klienten er ikke klar.");
                var query = new QueryExpression(KontoSystemLogicalName)
                {
                    ColumnSet = new ColumnSet(KontoSystemExpiryDate),
                    TopCount = 1,
                };
                query.Orders.Add(new OrderExpression(KontoSystemId, OrderType.Ascending));
                var entity = client.RetrieveMultiple(query).Entities.FirstOrDefault();
                var value = entity != null ? entity.GetAttributeValue<DateTime?>(KontoSystemExpiryDate) : null;
                if (!value.HasValue)
                {
                    return DateTime.UtcNow.AddYears(-10);
                }
                var local = value.Value.ToLocalTime();
                return new DateTime(local.Year, local.Month, local.Day, 23, 59, 59, DateTimeKind.Utc);
            }
        }

        private int GetBatchCount()
        {
            var values = RetrieveConfigurationValues(BatchCountConfig);
            string raw;
            if (values.TryGetValue(BatchCountConfig, out raw) && int.TryParse(raw, out var parsed) && parsed > 0)
            {
                return parsed;
            }
            return _settings.DefaultBatchCount;
        }

        private Dictionary<string, string> RetrieveConfigurationValues(string prefixOrName)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using (var client = _connectionFactory.CreateClient())
            {
                if (!client.IsReady) throw new InvalidOperationException(client.LastError ?? "Dataverse-klienten er ikke klar.");
                var query = new QueryExpression(ConfigLogicalName)
                {
                    ColumnSet = new ColumnSet(ConfigName, ConfigValue),
                    PageInfo = new PagingInfo { Count = 500, PageNumber = 1 }
                };
                if (prefixOrName.EndsWith("."))
                {
                    query.Criteria.AddCondition(ConfigName, ConditionOperator.Like, prefixOrName + "%");
                }
                else
                {
                    query.Criteria.AddCondition(ConfigName, ConditionOperator.Equal, prefixOrName);
                }
                while (true)
                {
                    var page = client.RetrieveMultiple(query);
                    foreach (var entity in page.Entities)
                    {
                        var name = entity.GetAttributeValue<string>(ConfigName);
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        values[name] = entity.GetAttributeValue<string>(ConfigValue) ?? string.Empty;
                    }
                    if (!page.MoreRecords) break;
                    query.PageInfo.PageNumber += 1;
                    query.PageInfo.PagingCookie = page.PagingCookie;
                }
            }
            return values;
        }
    }
}
