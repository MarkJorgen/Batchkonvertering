using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using dk.gi.app.konto.startarealtjek.Application.Abstractions;
using dk.gi.app.konto.startarealtjek.Application.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace dk.gi.app.konto.startarealtjek.Infrastructure.Crm
{
    public sealed class KontoStartArealTjekDataverseClient : IKontoStartArealTjekScanClient
    {
        private const string AccountLogicalName = "ap_konto";
        private const string StatusCodeLogicalName = "ap_statuskoder";
        private const string SubjectLogicalName = "subject";
        private const string SubjectExtensionLogicalName = "ap_emnextention";
        private const string ConfigLogicalName = "config_configurationsetting";

        private const string AccountId = "ap_kontoid";
        private const string AccountNumber = "ap_kontonr";
        private const string AccountInUseYear = "ap_ibrugtagningsr";
        private const string AccountExistingSubject = "ap_emneforarealtjek";
        private const string AccountLastArealCheck = "ap_arealtjekgennemfoert";
        private const string AccountStateCode = "statecode";
        private const string AccountStatusFraMapper = "ap_statusframapper";
        private const string AccountBindingType = "ap_bindingstype";
        private const string AccountPropertyType = "ap_ejendomstype";
        private const string AccountSmhus = "ap_smhus";
        private const string AccountLovgrundlag = "ap_lovgrundlag";
        private const string AccountMultipleInUseYears = "ap_flereibrugtagningsr";

        private const string StatusCodeAccountLookup = "ap_statuskodeid";
        private const string StatusCodeCode = "ap_statuskoder";

        private const string ConfigName = "config_name";
        private const string ConfigValue = "config_ntextcolumn";
        private const string ConfigStatusCode = "statuscode";
        private const string SubjectId = "subjectid";
        private const string SubjectExtensionSubjectId = "ap_emneid";
        private const string SubjectExtensionAllowSelfService = "ap_tilladkontoselvbetjening";

        private const string BatchConfigPrefix = "app.konto.arealtjek.";
        private const string ServiceBusConfigPrefix = "Azure.Service.Bus.Queue.Crm.Indbakke.";
        private const int StatusAktiv = 1;
        private const int KontoStatusFraMapperAktiv = 1;
        private const int KontoLovgrundlagFraJuli2015 = 1;
        private const int KontoEntityTypeCode = 10783;
        private const int IncidentEntityTypeCode = 112;
        private const int IncidentStateActive = 0;

        private readonly KontoStartArealTjekSettings _settings;
        private readonly ServiceClient _serviceClient;
        private readonly IOrganizationService _service;
        private readonly Action<string> _trace;
        private bool _disposed;

        public KontoStartArealTjekDataverseClient(KontoStartArealTjekSettings settings, string connectionString, int timeOutMinutes, Action<string> trace = null)
        {
            _settings = settings;
            _trace = trace;
            ServiceClient.MaxConnectionTimeout = TimeSpan.FromMinutes(Math.Max(1, timeOutMinutes));

            _serviceClient = new ServiceClient(connectionString);
            if (_serviceClient == null)
                throw new InvalidOperationException("Failed to connect to Dataverse. ServiceClient instance was null.");
            if (!_serviceClient.IsReady)
                throw new InvalidOperationException("Failed to connect to Dataverse. LastError='" + (_serviceClient.LastError ?? string.Empty) + "'. LastException='" + (_serviceClient.LastException != null ? _serviceClient.LastException.ToString() : string.Empty) + "'.");

            _service = (IOrganizationService)_serviceClient;
        }

        public KontoStartArealTjekExecutionSummary VerifyConnection()
            => new KontoStartArealTjekExecutionSummary(true, 0, 0, 0, 0, "CRM-forbindelse valideret. Der blev ikke udført workflow-kald i VERIFYCRM-mode.", "local dataverse sdk");

        public KontoStartArealTjekBatchSettings ResolveBatchSettings()
        {
            var values = RetrieveConfigurationValues(BatchConfigPrefix);

            int arealCheckYears = GetInt(values, BatchConfigPrefix + "antalaar", _settings != null ? _settings.DefaultArealCheckYears : 3);
            int batchAu = GetInt(values, BatchConfigPrefix + "antalprbatch.au", _settings != null ? _settings.DefaultBatchCountAlmindeligUdlejning : 0);
            int batchEf = GetInt(values, BatchConfigPrefix + "antalprbatch.ef", _settings != null ? _settings.DefaultBatchCountEjerforening : 0);
            int batchAb = GetInt(values, BatchConfigPrefix + "antalprbatch.ab", _settings != null ? _settings.DefaultBatchCountAndelsbolig : 0);

            return new KontoStartArealTjekBatchSettings(arealCheckYears, _settings != null ? _settings.DefaultBuildYearBefore : 1970, batchAu, batchEf, batchAb, "crm config_configurationsetting");
        }

        public ResolvedServiceBusSettings ResolveServiceBusSettings()
        {
            var values = RetrieveConfigurationValues(ServiceBusConfigPrefix);

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

        public IReadOnlyCollection<KontoStartArealTjekAssessment> AssessAccounts(KontoStartArealTjekRequest request, KontoStartArealTjekBatchSettings batchSettings)
        {
            var excludedSubjects = GetSelfServiceSubjectIds();
            var statusCode22AccountIds = GetStatusCode22AccountIds();
            var accounts = new List<KontoStartArealTjekAccount>();
            accounts.AddRange(GetAccountsForPropertyType(KontoStartArealTjekPropertyType.AlmindeligUdlejning, batchSettings.ArealCheckYears, batchSettings.BuildYearBefore, request.KontoNr));
            accounts.AddRange(GetAccountsForPropertyType(KontoStartArealTjekPropertyType.Ejerforening, batchSettings.ArealCheckYears, batchSettings.BuildYearBefore, request.KontoNr));
            accounts.AddRange(GetAccountsForPropertyType(KontoStartArealTjekPropertyType.Andelsbolig, batchSettings.ArealCheckYears, batchSettings.BuildYearBefore, request.KontoNr));

            var accountsWithOpenCases = GetAccountIdsWithOpenCases(accounts.Select(x => x.AccountId).ToList(), excludedSubjects);

            var result = new List<KontoStartArealTjekAssessment>(accounts.Count);
            int processed = 0;
            foreach (var account in accounts)
            {
                processed++;
                bool hasStatusCode22 = statusCode22AccountIds.Contains(account.AccountId);
                bool hasOpenCases = accountsWithOpenCases.Contains(account.AccountId);
                bool shouldBeSubject = !hasStatusCode22 && !hasOpenCases;
                result.Add(new KontoStartArealTjekAssessment(account, hasStatusCode22, hasOpenCases, shouldBeSubject));

                if (processed % 250 == 0 || processed == accounts.Count)
                {
                    _trace?.Invoke("Vurderede " + processed + " / " + accounts.Count + " konto(er) i arealtjek-flowet.");
                }
            }

            return result;
        }

        public void ApplySubjectFlagUpdates(IReadOnlyCollection<KontoStartArealTjekAssessment> assessments)
        {
            if (assessments == null)
            {
                _trace?.Invoke("Ingen assessments modtaget til opdatering af ap_emneforarealtjek.");
                return;
            }

            var pending = assessments
                .Where(x => x != null && x.Account != null && x.WillUpdateSubjectFlag)
                .ToList();

            _trace?.Invoke("Starter opdatering af ap_emneforarealtjek på " + pending.Count + " konto(er).");

            int updates = 0;
            int total = pending.Count;
            foreach (var assessment in pending)
            {
                if (updates < 5 || (updates + 1) % 100 == 0 || updates + 1 == total)
                {
                    _trace?.Invoke(
                        "Opdaterer ap_emneforarealtjek " + (updates + 1) + " / " + total +
                        " for konto " + assessment.Account.AccountNumber +
                        " (" + assessment.Account.AccountId + ") til værdi=" + (assessment.ShouldBeSubject ? "true" : "false") + ".");
                }

                try
                {
                    var entity = new Entity(AccountLogicalName) { Id = assessment.Account.AccountId };
                    entity[AccountExistingSubject] = assessment.ShouldBeSubject;
                    _service.Update(entity);
                    updates++;
                }
                catch (Exception ex)
                {
                    _trace?.Invoke(
                        "Fejl ved opdatering af ap_emneforarealtjek for konto " + assessment.Account.AccountNumber +
                        " (" + assessment.Account.AccountId + "). " + ex.GetType().FullName + ": " + ex.Message);
                    throw;
                }
            }

            _trace?.Invoke("Opdaterede ap_emneforarealtjek på " + updates + " konto(er).");
        }

        private List<KontoStartArealTjekAccount> GetAccountsForPropertyType(KontoStartArealTjekPropertyType propertyType, int arealCheckYears, int buildYearBefore, string kontoNr)
        {
            var query = new QueryExpression(AccountLogicalName)
            {
                ColumnSet = new ColumnSet(AccountId, AccountNumber, AccountInUseYear, AccountExistingSubject, AccountLastArealCheck, AccountPropertyType),
                Distinct = false,
                NoLock = true,
                PageInfo = new PagingInfo { PageNumber = 1, Count = 500 }
            };

            query.Criteria.AddCondition(AccountStateCode, ConditionOperator.Equal, 0);
            query.Criteria.AddCondition(AccountStatusFraMapper, ConditionOperator.Equal, KontoStatusFraMapperAktiv);
            query.Criteria.AddCondition(AccountBindingType, ConditionOperator.Equal, false);
            query.Criteria.AddCondition(AccountPropertyType, ConditionOperator.Equal, (int)propertyType);
            query.Criteria.AddCondition(AccountSmhus, ConditionOperator.Equal, false);
            query.Criteria.AddCondition(AccountLovgrundlag, ConditionOperator.Equal, KontoLovgrundlagFraJuli2015);
            query.Criteria.AddCondition(AccountMultipleInUseYears, ConditionOperator.Equal, false);
            if (!string.IsNullOrWhiteSpace(kontoNr))
                query.Criteria.AddCondition(AccountNumber, ConditionOperator.Equal, kontoNr);

            var arealFilter = new FilterExpression(LogicalOperator.Or);
            arealFilter.AddCondition(AccountLastArealCheck, ConditionOperator.Null);
            arealFilter.AddCondition(AccountLastArealCheck, ConditionOperator.OlderThanXYears, Math.Max(0, arealCheckYears));
            query.Criteria.AddFilter(arealFilter);
            query.AddOrder(AccountNumber, OrderType.Ascending);

            return RetrieveAll(query, "konto-startarealtjek-" + propertyType.ToString().ToLowerInvariant())
                .Select(entity => ToAccount(entity, propertyType))
                .Where(x => x != null && x.InUseYear.HasValue && x.InUseYear.Value < buildYearBefore)
                .ToList();
        }

        private KontoStartArealTjekAccount ToAccount(Entity entity, KontoStartArealTjekPropertyType propertyType)
        {
            if (entity == null)
                return null;

            Guid id = entity.Id;
            string kontoNr = entity.GetAttributeValue<string>(AccountNumber) ?? string.Empty;
            string inUseYearRaw = entity.GetAttributeValue<string>(AccountInUseYear) ?? string.Empty;
            bool? existingSubject = entity.Attributes.Contains(AccountExistingSubject)
                ? (bool?)entity.GetAttributeValue<bool>(AccountExistingSubject)
                : null;
            DateTime? lastArealCheck = entity.Attributes.Contains(AccountLastArealCheck)
                ? (DateTime?)entity.GetAttributeValue<DateTime>(AccountLastArealCheck)
                : null;

            int parsedYear;
            int? inUseYear = int.TryParse(inUseYearRaw, out parsedYear) ? (int?)parsedYear : null;
            return new KontoStartArealTjekAccount(id, kontoNr, propertyType, inUseYearRaw, inUseYear, existingSubject, lastArealCheck);
        }

        private HashSet<Guid> GetStatusCode22AccountIds()
        {
            var query = new QueryExpression(StatusCodeLogicalName)
            {
                ColumnSet = new ColumnSet(StatusCodeAccountLookup),
                Distinct = false,
                NoLock = true,
                PageInfo = new PagingInfo { PageNumber = 1, Count = 500 }
            };
            query.Criteria.AddCondition(StatusCodeCode, ConditionOperator.Equal, 22);

            return new HashSet<Guid>(
                RetrieveAll(query, "konto-startarealtjek-statuskode22")
                    .Select(x => x.GetAttributeValue<EntityReference>(StatusCodeAccountLookup))
                    .Where(x => x != null)
                    .Select(x => x.Id));
        }

        private HashSet<Guid> GetSelfServiceSubjectIds()
        {
            var query = new QueryExpression(SubjectLogicalName)
            {
                ColumnSet = new ColumnSet(SubjectId),
                Distinct = true,
                NoLock = true,
                PageInfo = new PagingInfo { PageNumber = 1, Count = 500 }
            };

            query.AddLink(SubjectExtensionLogicalName, SubjectId, SubjectExtensionSubjectId)
                .LinkCriteria.AddCondition(SubjectExtensionAllowSelfService, ConditionOperator.Equal, true);

            return new HashSet<Guid>(RetrieveAll(query, "konto-startarealtjek-selfservice-subjects").Select(x => x.Id));
        }

        private HashSet<Guid> GetAccountIdsWithOpenCases(IReadOnlyCollection<Guid> accountIds, HashSet<Guid> excludedSubjectIds)
        {
            var result = new HashSet<Guid>();
            if (accountIds == null || accountIds.Count == 0)
                return result;

            var ids = accountIds.Where(x => x != Guid.Empty).Distinct().ToList();
            const int chunkSize = 200;
            int totalChunks = (ids.Count + chunkSize - 1) / chunkSize;

            for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
            {
                var chunk = ids.Skip(chunkIndex * chunkSize).Take(chunkSize).ToList();
                if (chunk.Count == 0)
                    continue;

                string excluded = BuildExcludedSubjectsCondition(excludedSubjectIds);
                string accountIdValues = string.Join(Environment.NewLine, chunk.Select(x => "<value>" + x.ToString("D") + "</value>"));

                string fetch = string.Join(string.Empty,
                    "<fetch version='1.0' aggregate='true'>",
                        "<entity name='connection'>",
                            "<attribute name='record1id' alias='kontoid' groupby='true' />",
                            "<filter>",
                                "<condition attribute='record1objecttypecode' operator='eq' value='", KontoEntityTypeCode.ToString(), "' />",
                                "<condition attribute='record2objecttypecode' operator='eq' value='", IncidentEntityTypeCode.ToString(), "' />",
                                "<condition attribute='record1id' operator='in'>",
                                    accountIdValues,
                                "</condition>",
                            "</filter>",
                            "<link-entity name='incident' from='incidentid' to='record2id'>",
                                "<filter>",
                                    "<condition attribute='statecode' operator='eq' value='", IncidentStateActive.ToString(), "' />",
                                    excluded,
                                "</filter>",
                            "</link-entity>",
                        "</entity>",
                    "</fetch>");

                DateTime started = DateTime.UtcNow;
                var response = _service.RetrieveMultiple(new FetchExpression(fetch));
                double seconds = (DateTime.UtcNow - started).TotalSeconds;

                foreach (var entity in response.Entities)
                {
                    if (!entity.Attributes.Contains("kontoid"))
                        continue;

                    Guid? accountId = TryGetGroupedGuid(entity["kontoid"]);
                    if (accountId.HasValue && accountId.Value != Guid.Empty)
                        result.Add(accountId.Value);
                }

                _trace?.Invoke(
                    "Query 'konto-startarealtjek-open-cases' chunk " + (chunkIndex + 1) + " / " + totalChunks +
                    " hentet på " + seconds.ToString("0.0") +
                    " sek. ChunkStørrelse=" + chunk.Count +
                    ", kontiMedÅbneSager=" + result.Count);
            }

            return result;
        }

        private static string BuildExcludedSubjectsCondition(HashSet<Guid> excludedSubjectIds)
        {
            if (excludedSubjectIds == null || excludedSubjectIds.Count == 0)
                return string.Empty;

            return string.Join(string.Empty,
                "<condition attribute='subjectid' operator='not-in'>",
                string.Join(Environment.NewLine, excludedSubjectIds.Select(x => "<value>" + x.ToString("D") + "</value>")),
                "</condition>");
        }

        private static Guid? TryGetGroupedGuid(object value)
        {
            if (value == null)
                return null;

            if (value is AliasedValue aliased)
                value = aliased.Value;

            if (value is Guid guid)
                return guid;

            if (value is EntityReference reference)
                return reference.Id;

            if (value is string raw && Guid.TryParse(raw, out Guid parsed))
                return parsed;

            return null;
        }

        private Dictionary<string, string> RetrieveConfigurationValues(string prefix)
        {
            var query = new QueryExpression(ConfigLogicalName)
            {
                ColumnSet = new ColumnSet(ConfigName, ConfigValue),
                Distinct = false,
                NoLock = true,
                PageInfo = new PagingInfo { PageNumber = 1, Count = 100 }
            };
            query.Criteria.AddCondition(ConfigStatusCode, ConditionOperator.Equal, StatusAktiv);
            query.Criteria.AddCondition(ConfigName, ConditionOperator.Like, prefix + "%");
            query.AddOrder(ConfigName, OrderType.Ascending);

            return RetrieveAll(query, prefix.Contains("Azure.Service.Bus") ? "crm-servicebus-settings" : "crm-arealtjek-settings")
                .Where(x => x.Attributes.Contains(ConfigName) && x.Attributes.Contains(ConfigValue))
                .ToDictionary(
                    x => x.GetAttributeValue<string>(ConfigName) ?? string.Empty,
                    x => x.GetAttributeValue<string>(ConfigValue) ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);
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
                    break;

                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = response.PagingCookie;
            }

            return result;
        }

        private static int GetInt(Dictionary<string, string> values, string key, int defaultValue)
        {
            if (values != null && values.TryGetValue(key, out string raw) && int.TryParse(raw, out int parsed))
                return parsed;
            return defaultValue;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _serviceClient.Dispose();
            _disposed = true;
        }
    }
}
