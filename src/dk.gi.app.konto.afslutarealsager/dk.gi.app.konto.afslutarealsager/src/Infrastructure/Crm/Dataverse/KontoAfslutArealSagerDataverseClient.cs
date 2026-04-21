using System;
using System.Collections.Generic;
using System.Linq;
using dk.gi.app.konto.afslutarealsager.Application.Abstractions;
using dk.gi.app.konto.afslutarealsager.Application.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Crm.Dataverse
{
    public sealed class KontoAfslutArealSagerDataverseClient : IKontoAfslutArealSagerScanClient
    {
        private const string IncidentLogicalName = "incident";
        private const string IncidentResolutionLogicalName = "incidentresolution";
        private const string SubjectLogicalName = "subject";
        private const string SystemUserLogicalName = "systemuser";
        private const string ConnectionLogicalName = "connection";
        private const string AccountLogicalName = "ap_konto";
        private const string PropertyLogicalName = "ap_ejendom";
        private const string ContactLogicalName = "contact";
        private const string TaskLogicalName = "task";
        private const string AnnotationLogicalName = "annotation";
        private const string ConfigLogicalName = "config_configurationsetting";

        private const string IncidentId = "incidentid";
        private const string IncidentCaseNumber = "ap_sagsnummer";
        private const string IncidentFallbackCaseNumber = "ticketnumber";
        private const string IncidentSubjectId = "subjectid";
        private const string IncidentOwnerId = "ownerid";
        private const string IncidentFollowupBy = "followupby";
        private const string IncidentCreatedOn = "createdon";
        private const string IncidentStateCode = "statecode";

        private const string IncidentResolutionSubject = "subject";
        private const string IncidentResolutionDescription = "description";
        private const string IncidentResolutionIncidentId = "incidentid";

        private const string SubjectId = "subjectid";
        private const string SubjectTitle = "title";
        private const string SubjectParent = "parentsubject";

        private const string SystemUserId = "systemuserid";
        private const string SystemUserDomainName = "domainname";
        private const string SystemUserEmail = "internalemailaddress";
        private const string SystemUserFullName = "fullname";
        private const string SystemUserDisabled = "isdisabled";

        private const string ConnectionRecord1Id = "record1id";
        private const string ConnectionRecord2Id = "record2id";
        private const string ConnectionRecord1Type = "record1objecttypecode";
        private const string ConnectionRecord2Type = "record2objecttypecode";

        private const string AccountNumber = "ap_kontonr";
        private const string AccountPrimaryContact = "ap_primrkontaktid";
        private const string AccountProperty = "ap_ejendomid";
        private const string AccountLastAccountingDate = "ap_sidsteregnskabsdato";

        private const string PropertyAddress = "ap_samletadresse";

        private const string ContactCompanyId = "ap_virksomhedsid";
        private const string ContactGovernmentId = "governmentid";
        private const string ContactFullName = "ap_fullname";
        private const string ContactAddress1 = "address1_line1";
        private const string ContactPostalCode = "address1_postalcode";
        private const string ContactCity = "address1_city";

        private const string TaskSubject = "subject";
        private const string TaskDescription = "description";
        private const string TaskRegardingObjectId = "regardingobjectid";
        private const string TaskScheduledEnd = "scheduledend";
        private const string TaskActualEnd = "actualend";

        private const string AnnotationSubject = "subject";
        private const string AnnotationNoteText = "notetext";
        private const string AnnotationObjectId = "objectid";
        private const string AnnotationIsDocument = "isdocument";
        private const string AnnotationDocumentBody = "documentbody";
        private const string AnnotationFileName = "filename";
        private const string AnnotationMimeType = "mimetype";

        private const string ConfigName = "config_name";
        private const string ConfigValue = "config_ntextcolumn";
        private const string AreaLogicalName = "ap_areal";
        private const string AreaId = "ap_arealid";
        private const string AreaAccountRef = "ap_arealerid";
        private const string AreaPeriodStart = "ap_periodestart";
        private const string AreaPeriodEnd = "ap_periodeslut";
        private const string AreaPropertyRef = "ap_ejendom";
        private const string AreaPropertyTypeRef = "ap_ejendomstype";
        private const string AreaAccountNumber = "ap_kontonr";
        private const string AreaBbrResidential = "ap_bbrbeboelsesareal";
        private const string AreaBbrCommercial = "ap_bbrerhvervsareal";
        private const string AreaResidentialBefore1964 = "ap_beboelsesarealopfrtfr1964";
        private const string AreaResidential1963To1969 = "ap_beboelsesarealopfrtefter1963";
        private const string AreaResidentialAfter1969 = "ap_beboelsesarealopfrtefter1969";
        private const string AreaCommercialBefore1964 = "ap_erhvervarealopfrtfr1964";
        private const string AreaCommercial1963To1969 = "ap_erhvervsareaopfrtefter1963";
        private const string AreaCommercialAfter1969 = "ap_erhvervsarealopfrtefter1969";
        private const string AreaResidentialHighSupplement = "ap_beboelsesarealhjttillg";
        private const string AreaResidentialLowSupplement = "ap_beboelsesareallavttillg";
        private const string AreaCommercialHighSupplement = "ap_erhvervaarealhjttillg";
        private const string AreaCommercialLowSupplement = "ap_erhvervsareallavttillg";
        private const string AreaResidentialUnits = "ap_beboelseslejeml";
        private const string AreaCommercialUnits = "ap_erhvervslejeml";
        private const string AreaGiDistribution = "ap_fordelingstalialtgi";
        private const string AreaExempt1963To1969 = "ap_fritagetarealopfrtefter1963";
        private const string AreaExemptBefore1964 = "ap_fritagetarealopfrtfr1964";
        private const string AreaTotal = "ap_samletareal";
        private const string AreaTotalDistribution = "ap_samletfordelingstal";

        private const string RegnskabLogicalName = "ap_regnskab";
        private const string RegnskabId = "ap_regnskabid";
        private const string RegnskabAccountRef = "ap_kontoid";
        private const string RegnskabReasonRef = "ap_regnskabrsagid";
        private const string RegnskabReasonLogicalName = "ap_regnskabsrsager";
        private const string RegnskabReasonId = "ap_regnskabsrsagerid";
        private const string RegnskabReasonCode = "ap_kode";
        private const string ConfigStatusCode = "statuscode";
        private const string ServiceBusConfigPrefix = "Azure.Service.Bus.Queue.Crm.Indbakke.";
        private const int StatusAktiv = 1;

        private readonly ServiceClient _serviceClient;
        private readonly Action<string> _trace;

        public KontoAfslutArealSagerDataverseClient(KontoAfslutArealSagerSettings settings, string connectionString, int timeOutMinutes, Action<string> trace = null)
        {
            _trace = trace ?? (_ => { });
            ServiceClient.MaxConnectionTimeout = TimeSpan.FromMinutes(Math.Max(1, timeOutMinutes));
            _serviceClient = new ServiceClient(connectionString);
            if (_serviceClient == null)
                throw new InvalidOperationException("Failed to connect to Dataverse. ServiceClient instance was null.");
            if (!_serviceClient.IsReady)
                throw new InvalidOperationException("Failed to connect to Dataverse. LastError='" + (_serviceClient.LastError ?? string.Empty) + "'. LastException='" + (_serviceClient.LastException != null ? _serviceClient.LastException.ToString() : string.Empty) + "'.");
        }

        public void EnsureConnection()
        {
            WhoAmIResponse response = (WhoAmIResponse)_serviceClient.Execute(new WhoAmIRequest());
            _trace("WhoAmI succeeded. UserId=" + response.UserId.ToString("D"));
        }

        public IReadOnlyList<KontoAfslutArealSagerCandidate> GetOpenCases(KontoAfslutArealSagerRequest request)
        {
            Guid userId = ResolveUserId(request.BrugerArealSager);
            Guid subjectId = ResolveSubjectId("Konto", "Arealændring");
            DateTime followupThreshold = DateTime.Now.AddDays(request.OpfoelgesFraPlusDage);

            List<Entity> incidents;
            if (request.HasForcedAccountSelector && !request.HasForcedCaseSelector)
            {
                _trace("Resolved filters. ForceKontonr aktiv. UserId=" + userId.ToString("D") + ", SubjectId=" + subjectId.ToString("D") + ", ForceKontonr=" + SafeValue(request.ForceKontonr) + ". Owner/subject/followup omgås for målrettet teknisk verifikation.");
                incidents = GetActiveIncidentsForAccountNumber(request.ForceKontonr);
            }
            else
            {
                var query = new QueryExpression(IncidentLogicalName)
                {
                    ColumnSet = new ColumnSet(IncidentId, IncidentCaseNumber, IncidentFallbackCaseNumber, IncidentCreatedOn, IncidentSubjectId),
                    PageInfo = new PagingInfo { Count = 500, PageNumber = 1 }
                };
                query.Criteria.AddCondition(IncidentStateCode, ConditionOperator.Equal, 0);

                if (request.HasForcedCaseSelector)
                {
                    _trace("Resolved filters. Force selector aktiv. UserId=" + userId.ToString("D") + ", SubjectId=" + subjectId.ToString("D") + ", ForceIncidentId=" + SafeValue(request.ForceIncidentId) + ", ForceSagsnummer=" + SafeValue(request.ForceSagsnummer) + ", ForceKontonr=" + SafeValue(request.ForceKontonr));

                    if (Guid.TryParse(request.ForceIncidentId, out Guid forcedIncidentId))
                    {
                        query.Criteria.AddCondition(IncidentId, ConditionOperator.Equal, forcedIncidentId);
                    }

                    if (!string.IsNullOrWhiteSpace(request.ForceSagsnummer))
                    {
                        var caseFilter = new FilterExpression(LogicalOperator.Or);
                        caseFilter.AddCondition(IncidentCaseNumber, ConditionOperator.Equal, request.ForceSagsnummer);
                        caseFilter.AddCondition(IncidentFallbackCaseNumber, ConditionOperator.Equal, request.ForceSagsnummer);
                        query.Criteria.AddFilter(caseFilter);
                    }
                }
                else
                {
                    _trace("Resolved filters. UserId=" + userId.ToString("D") + ", SubjectId=" + subjectId.ToString("D") + ", FollowupBy<=" + followupThreshold.ToString("yyyy-MM-dd HH:mm:ss") + ", ForceKontonr=" + SafeValue(request.ForceKontonr));
                    query.Criteria.AddCondition(IncidentOwnerId, ConditionOperator.Equal, userId);
                    query.Criteria.AddCondition(IncidentSubjectId, ConditionOperator.Equal, subjectId);
                    query.Criteria.AddCondition(IncidentFollowupBy, ConditionOperator.LessEqual, followupThreshold);
                }

                incidents = RetrieveAll(query, "incidents");
            }

            var result = new List<KontoAfslutArealSagerCandidate>();
            foreach (var incident in incidents)
            {
                if (TryBuildCandidate(incident, out var candidate))
                {
                    if (string.IsNullOrWhiteSpace(request.ForceKontonr) || string.Equals(candidate.AccountNumber, request.ForceKontonr, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(candidate);
                    }
                }
            }

            if (result.Count == 0)
            {
                TraceNoMatchDiagnostics(userId, subjectId, followupThreshold, request);
            }

            return result;
        }

        public IReadOnlyList<KontoAfslutArealSagerCandidate> DiscoverCases(KontoAfslutArealSagerRequest request, int limit)
        {
            int effectiveLimit = Math.Max(1, Math.Min(limit, 200));
            Guid subjectId = ResolveSubjectId("Konto", "Arealændring");

            var query = new QueryExpression(IncidentLogicalName)
            {
                ColumnSet = new ColumnSet(IncidentId, IncidentCaseNumber, IncidentFallbackCaseNumber, IncidentCreatedOn, IncidentSubjectId, IncidentOwnerId, IncidentFollowupBy),
                TopCount = effectiveLimit
            };
            query.Criteria.AddCondition(IncidentStateCode, ConditionOperator.Equal, 0);
            query.Criteria.AddCondition(IncidentSubjectId, ConditionOperator.Equal, subjectId);
            query.AddOrder(IncidentCreatedOn, OrderType.Descending);

            _trace("Discovery-mode query. Active incidents for emnet 'Konto / Arealændring' uden owner-filter. Limit=" + effectiveLimit + ".");
            var incidents = _serviceClient.RetrieveMultiple(query).Entities.ToList();
            _trace("Discovery-mode raw incidents fetched=" + incidents.Count + ".");

            var result = new List<KontoAfslutArealSagerCandidate>();
            foreach (var incident in incidents)
            {
                if (TryBuildCandidate(incident, out var candidate))
                {
                    if (!string.IsNullOrWhiteSpace(request != null ? request.ForceKontonr : null)
                        && !string.Equals(candidate.AccountNumber, request.ForceKontonr, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    result.Add(candidate);
                    _trace("Discovery[" + result.Count + "] IncidentId=" + incident.Id.ToString("D")
                        + ", Sagsnr=" + SafeValue(candidate.CaseNumber)
                        + ", Kontonr=" + SafeValue(candidate.AccountNumber)
                        + ", Owner=" + SafeEntityReferenceName(incident.GetAttributeValue<EntityReference>(IncidentOwnerId))
                        + ", Subject=" + SafeEntityReferenceName(incident.GetAttributeValue<EntityReference>(IncidentSubjectId))
                        + ", FollowupBy=" + SafeDate(incident.GetAttributeValue<DateTime?>(IncidentFollowupBy))
                        + ", Oprettet=" + SafeDate(incident.GetAttributeValue<DateTime?>(IncidentCreatedOn)));
                }
                else
                {
                    _trace("Discovery skip. IncidentId=" + incident.Id.ToString("D")
                        + ", Sagsnr=" + SafeValue(ReadString(incident, IncidentCaseNumber))
                        + ", Owner=" + SafeEntityReferenceName(incident.GetAttributeValue<EntityReference>(IncidentOwnerId))
                        + ", FollowupBy=" + SafeDate(incident.GetAttributeValue<DateTime?>(IncidentFollowupBy))
                        + ". Kandidaten kunne ikke bygges via connection/konto-sporet.");
                }
            }

            if (result.Count == 0)
            {
                int subjectCount = CountIncidents(q =>
                {
                    q.Criteria.AddCondition(IncidentSubjectId, ConditionOperator.Equal, subjectId);
                });

                _trace("Discovery fandt 0 byggebare kandidater. ActiveForSubject=" + subjectCount + ", ForceKontonr=" + SafeValue(request != null ? request.ForceKontonr : null) + ".");
            }

            return result;
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

        public Guid CreateLetterActivity(KontoAfslutArealSagerCandidate candidate, KontoAfslutArealSagerRequest request)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));

            var activity = new Entity(TaskLogicalName);
            activity[TaskSubject] = "Areal - Brev automatisk afslutning af små arealsager";
            activity[TaskDescription] = "Brev automatisk afslutning af små arealsager";
            activity[TaskRegardingObjectId] = new EntityReference(IncidentLogicalName, candidate.CaseId);
            activity[TaskScheduledEnd] = DateTime.Now;

            Guid activityId = _serviceClient.Create(activity);
            _trace("Oprettede partial-run aktivitet for sag " + candidate.CaseNumber + ": " + activityId.ToString("D"));
            return activityId;
        }

        public void UploadLetterToActivity(Guid activityId, KontoAfslutArealSagerCandidate candidate, byte[] pdfBytes)
        {
            if (activityId == Guid.Empty) throw new ArgumentException("AktivitetsId mangler værdi.", nameof(activityId));
            if (pdfBytes == null || pdfBytes.Length == 0) throw new ArgumentException("PdfBytes mangler værdi.", nameof(pdfBytes));

            string caseNumber = candidate != null && !string.IsNullOrWhiteSpace(candidate.CaseNumber) ? candidate.CaseNumber : "ukendt";

            var note = new Entity(AnnotationLogicalName);
            note[AnnotationSubject] = "Areal - Brev automatisk afslutning af små arealsager";
            note[AnnotationNoteText] = "Partial RUN: vedhæftet brev til aktivitet for sag " + caseNumber + ".";
            note[AnnotationObjectId] = new EntityReference(TaskLogicalName, activityId);
            note[AnnotationIsDocument] = true;
            note[AnnotationDocumentBody] = Convert.ToBase64String(pdfBytes);
            note[AnnotationFileName] = "automatisk afslutning af små arealsager.pdf";
            note[AnnotationMimeType] = "application/pdf";

            Guid noteId = _serviceClient.Create(note);
            _trace("Vedhæftede brev som note " + noteId.ToString("D") + " til aktivitet " + activityId.ToString("D") + ".");
        }

        public void CompleteActivity(Guid activityId)
        {
            if (activityId == Guid.Empty) throw new ArgumentException("AktivitetsId mangler værdi.", nameof(activityId));

            var update = new Entity(TaskLogicalName) { Id = activityId };
            update[TaskActualEnd] = DateTime.Now;
            _serviceClient.Update(update);

            _serviceClient.Execute(new SetStateRequest
            {
                EntityMoniker = new EntityReference(TaskLogicalName, activityId),
                State = new OptionSetValue(1),
                Status = new OptionSetValue(5)
            });

            _trace("Lukkede aktivitet " + activityId.ToString("D") + " som completed.");
        }

        public void StageDigitalPostNote(Guid activityId, KontoAfslutArealSagerCandidate candidate, byte[] pdfBytes, string title)
        {
            if (activityId == Guid.Empty) throw new ArgumentException("AktivitetsId mangler værdi.", nameof(activityId));
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));

            string recipientType = string.IsNullOrWhiteSpace(candidate.CompanyId) ? "CPR" : "CVR";
            string recipientId = string.IsNullOrWhiteSpace(candidate.CompanyId) ? candidate.GovernmentId : candidate.CompanyId;
            string noteText = "Phase 6 digital-post staging. Ingen ekstern afsendelse endnu. Sag=" + candidate.CaseNumber
                + ", Modtagertype=" + recipientType
                + ", Modtager=" + SafeValue(recipientId)
                + ", Titel=" + (string.IsNullOrWhiteSpace(title) ? "Ændring af areal" : title)
                + ", PdfBytes=" + (pdfBytes != null ? pdfBytes.Length.ToString() : "0");

            var note = new Entity(AnnotationLogicalName);
            note[AnnotationSubject] = "Digital post staging - areal";
            note[AnnotationNoteText] = noteText;
            note[AnnotationObjectId] = new EntityReference(TaskLogicalName, activityId);
            note[AnnotationIsDocument] = false;

            Guid noteId = _serviceClient.Create(note);
            _trace("Staged digital post som note " + noteId.ToString("D") + " for aktivitet " + activityId.ToString("D") + ".");
        }

        public void CloseIncident(KontoAfslutArealSagerCandidate candidate, int statusCode, string resolutionSubject, string resolutionDescription)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));
            if (candidate.CaseId == Guid.Empty) throw new ArgumentException("CaseId mangler værdi.", nameof(candidate));

            var resolution = new Entity(IncidentResolutionLogicalName);
            resolution[IncidentResolutionSubject] = string.IsNullOrWhiteSpace(resolutionSubject) ? "Luk areal check" : resolutionSubject;
            resolution[IncidentResolutionDescription] = string.IsNullOrWhiteSpace(resolutionDescription) ? "Partial RUN direkte incident-closeout" : resolutionDescription;
            resolution[IncidentResolutionIncidentId] = new EntityReference(IncidentLogicalName, candidate.CaseId);

            _serviceClient.Execute(new CloseIncidentRequest
            {
                IncidentResolution = resolution,
                Status = new OptionSetValue(statusCode)
            });

            _trace("Lukkede incident direkte i Dataverse for sag " + candidate.CaseNumber + " (" + candidate.CaseId.ToString("D") + ") med statuskode " + statusCode + ".");
        }

        public KontoAfslutArealSagerArealCarryForwardResult CarryForwardOpenArea(KontoAfslutArealSagerCandidate candidate, bool deleteZeroRegnskab)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));
            if (!candidate.LastAccountingDate.HasValue)
            {
                return KontoAfslutArealSagerArealCarryForwardResult.Skipped(candidate.AccountNumber, "Konto mangler ap_sidsteregnskabsdato.");
            }

            var area = FindOpenArea(candidate.AccountId);
            if (area == null)
            {
                return KontoAfslutArealSagerArealCarryForwardResult.Skipped(candidate.AccountNumber, "Åbent ap_areal blev ikke fundet.");
            }

            if (!IsLegacyRelevantArea(area))
            {
                return KontoAfslutArealSagerArealCarryForwardResult.Skipped(candidate.AccountNumber, "Arealet er ikke relevant for small-difference closeout (kun efter-1969 arealer).");
            }

            DateTime closeDate = candidate.LastAccountingDate.Value.ToLocalTime();
            DateTime newStartDate = closeDate.AddDays(1);

            var closeEntity = new Entity(AreaLogicalName) { Id = area.Id };
            closeEntity[AreaPeriodEnd] = closeDate;
            _serviceClient.Update(closeEntity);

            var newArea = new Entity(AreaLogicalName);
            CopyIfPresent(area, newArea, AreaAccountRef, AreaPropertyRef, AreaPropertyTypeRef, AreaAccountNumber,
                AreaBbrResidential, AreaBbrCommercial,
                AreaResidentialBefore1964, AreaResidential1963To1969, AreaResidentialAfter1969,
                AreaCommercialBefore1964, AreaCommercial1963To1969, AreaCommercialAfter1969,
                AreaResidentialHighSupplement, AreaResidentialLowSupplement,
                AreaCommercialHighSupplement, AreaCommercialLowSupplement,
                AreaResidentialUnits, AreaCommercialUnits,
                AreaGiDistribution, AreaExempt1963To1969, AreaExemptBefore1964,
                AreaTotal, AreaTotalDistribution);
            newArea[AreaPeriodStart] = newStartDate;
            newArea[AreaPeriodEnd] = null;
            Guid newAreaId = _serviceClient.Create(newArea);

            bool deleted = false;
            if (deleteZeroRegnskab)
            {
                deleted = TryDeleteZeroRegnskab(candidate.AccountId);
            }

            _trace("Carry-forward af ap_areal gennemført for konto " + candidate.AccountNumber + ". Gammelt areal lukket pr. " + closeDate.ToString("yyyy-MM-dd") + ", nyt areal=" + newAreaId.ToString("D") + ".");
            return KontoAfslutArealSagerArealCarryForwardResult.Completed(candidate.AccountNumber, deleted, newAreaId.ToString("D"), "Åbent areal lukket og nyt areal oprettet med carry-forward af eksisterende feltværdier.");
        }

        private List<Entity> GetActiveIncidentsForAccountNumber(string accountNumber)
        {
            Guid accountId = ResolveAccountId(accountNumber);
            var incidents = new List<Entity>();
            foreach (Guid incidentId in FindIncidentIdsForAccount(accountId))
            {
                Entity incident = _serviceClient.Retrieve(IncidentLogicalName, incidentId, new ColumnSet(IncidentId, IncidentCaseNumber, IncidentFallbackCaseNumber, IncidentCreatedOn, IncidentSubjectId, IncidentStateCode));
                if (incident != null)
                {
                    OptionSetValue state = incident.GetAttributeValue<OptionSetValue>(IncidentStateCode);
                    if (state != null && state.Value == 0)
                    {
                        incidents.Add(incident);
                    }
                }
            }

            _trace("RetrieveAll 'incidents-forcekontonr' fetched=" + incidents.Count + " total=" + incidents.Count);
            return incidents;
        }

        private Guid ResolveAccountId(string accountNumber)
        {
            var query = new QueryExpression(AccountLogicalName)
            {
                ColumnSet = new ColumnSet(AccountNumber),
                TopCount = 2
            };
            query.Criteria.AddCondition(AccountNumber, ConditionOperator.Equal, accountNumber);
            Entity entity = _serviceClient.RetrieveMultiple(query).Entities.FirstOrDefault();
            if (entity == null)
            {
                throw new InvalidOperationException("Kunne ikke finde konto for ForceKontonr='" + accountNumber + "'.");
            }

            return entity.Id;
        }

        private IEnumerable<Guid> FindIncidentIdsForAccount(Guid accountId)
        {
            var query = new QueryExpression(ConnectionLogicalName)
            {
                ColumnSet = new ColumnSet(ConnectionRecord1Id, ConnectionRecord2Id),
                PageInfo = new PagingInfo { Count = 250, PageNumber = 1 }
            };
            var filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition(ConnectionRecord1Id, ConditionOperator.Equal, accountId);
            filter.AddCondition(ConnectionRecord2Id, ConditionOperator.Equal, accountId);
            query.Criteria.AddFilter(filter);

            var ids = new HashSet<Guid>();
            foreach (var connection in RetrieveAll(query, "connections-forcekontonr"))
            {
                var record1 = connection.GetAttributeValue<EntityReference>(ConnectionRecord1Id);
                var record2 = connection.GetAttributeValue<EntityReference>(ConnectionRecord2Id);
                if (record1 != null && string.Equals(record1.LogicalName, IncidentLogicalName, StringComparison.OrdinalIgnoreCase))
                {
                    ids.Add(record1.Id);
                }
                if (record2 != null && string.Equals(record2.LogicalName, IncidentLogicalName, StringComparison.OrdinalIgnoreCase))
                {
                    ids.Add(record2.Id);
                }
            }

            return ids;
        }

        private Entity FindOpenArea(Guid accountId)
        {
            var query = new QueryExpression(AreaLogicalName)
            {
                ColumnSet = new ColumnSet(AreaId, AreaAccountRef, AreaPeriodStart, AreaPeriodEnd, AreaPropertyRef, AreaPropertyTypeRef, AreaAccountNumber,
                    AreaBbrResidential, AreaBbrCommercial,
                    AreaResidentialBefore1964, AreaResidential1963To1969, AreaResidentialAfter1969,
                    AreaCommercialBefore1964, AreaCommercial1963To1969, AreaCommercialAfter1969,
                    AreaResidentialHighSupplement, AreaResidentialLowSupplement,
                    AreaCommercialHighSupplement, AreaCommercialLowSupplement,
                    AreaResidentialUnits, AreaCommercialUnits,
                    AreaGiDistribution, AreaExempt1963To1969, AreaExemptBefore1964,
                    AreaTotal, AreaTotalDistribution),
                TopCount = 1
            };
            query.Criteria.AddCondition(AreaAccountRef, ConditionOperator.Equal, accountId);
            query.Criteria.AddCondition(AreaPeriodEnd, ConditionOperator.Null);
            query.AddOrder(AreaPeriodStart, OrderType.Descending);
            return _serviceClient.RetrieveMultiple(query).Entities.FirstOrDefault();
        }

        private bool IsLegacyRelevantArea(Entity area)
        {
            return GetDouble(area, AreaResidentialBefore1964) > 0
                || GetDouble(area, AreaResidential1963To1969) > 0
                || GetDouble(area, AreaCommercialBefore1964) > 0
                || GetDouble(area, AreaCommercial1963To1969) > 0;
        }

        private bool TryDeleteZeroRegnskab(Guid accountId)
        {
            var query = new QueryExpression(RegnskabLogicalName)
            {
                ColumnSet = new ColumnSet(RegnskabId),
                TopCount = 1
            };
            query.Criteria.AddCondition(RegnskabAccountRef, ConditionOperator.Equal, accountId);
            var reasonLink = query.AddLink(RegnskabReasonLogicalName, RegnskabReasonRef, RegnskabReasonId, JoinOperator.Inner);
            reasonLink.LinkCriteria.AddCondition(RegnskabReasonCode, ConditionOperator.Equal, "02");
            Entity zeroRegnskab = _serviceClient.RetrieveMultiple(query).Entities.FirstOrDefault();
            if (zeroRegnskab == null)
            {
                _trace("Ingen 0-regnskab med kode 02 fundet for konto " + accountId.ToString("D") + ".");
                return false;
            }

            _serviceClient.Delete(RegnskabLogicalName, zeroRegnskab.Id);
            _trace("Slettede 0-regnskab " + zeroRegnskab.Id.ToString("D") + ".");
            return true;
        }

        private static void CopyIfPresent(Entity source, Entity target, params string[] attributes)
        {
            foreach (string attribute in attributes)
            {
                if (source.Contains(attribute) && source[attribute] != null)
                {
                    target[attribute] = source[attribute];
                }
            }
        }

        private static double GetDouble(Entity entity, string attributeName)
        {
            if (!entity.Contains(attributeName) || entity[attributeName] == null)
            {
                return 0d;
            }

            object value = entity[attributeName];
            if (value is double d) return d;
            if (value is decimal m) return (double)m;
            if (value is int i) return i;
            if (double.TryParse(value.ToString(), out double parsed)) return parsed;
            return 0d;
        }

        private Guid ResolveUserId(string userNameOrEmail)
        {
            var query = new QueryExpression(SystemUserLogicalName)
            {
                ColumnSet = new ColumnSet(SystemUserId),
                TopCount = 1
            };
            query.Criteria.AddCondition(SystemUserDisabled, ConditionOperator.Equal, false);
            var filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition(SystemUserDomainName, ConditionOperator.Equal, userNameOrEmail);
            filter.AddCondition(SystemUserEmail, ConditionOperator.Equal, userNameOrEmail);
            filter.AddCondition(SystemUserFullName, ConditionOperator.Equal, userNameOrEmail);
            query.Criteria.AddFilter(filter);

            Entity entity = _serviceClient.RetrieveMultiple(query).Entities.FirstOrDefault();
            if (entity == null)
                throw new InvalidOperationException("Kunne ikke finde systembruger for BrugerArealSager='" + userNameOrEmail + "'.");
            return entity.Id;
        }

        private Guid ResolveSubjectId(string parentTitle, string title)
        {
            var query = new QueryExpression(SubjectLogicalName)
            {
                ColumnSet = new ColumnSet(SubjectId, SubjectTitle),
                TopCount = 2
            };
            query.Criteria.AddCondition(SubjectTitle, ConditionOperator.Equal, title);
            var parent = query.AddLink(SubjectLogicalName, SubjectParent, SubjectId, JoinOperator.Inner);
            parent.EntityAlias = "parent";
            parent.LinkCriteria.AddCondition(SubjectTitle, ConditionOperator.Equal, parentTitle);

            Entity entity = _serviceClient.RetrieveMultiple(query).Entities.FirstOrDefault();
            if (entity == null)
                throw new InvalidOperationException("Kunne ikke finde emnet '" + parentTitle + " / " + title + "'.");
            return entity.Id;
        }

        private bool TryBuildCandidate(Entity incident, out KontoAfslutArealSagerCandidate candidate)
        {
            candidate = null;
            Guid caseId = incident.Id;
            EntityReference accountReference = FindAccountReference(caseId);
            if (accountReference == null)
            {
                _trace("Ingen konto-forbindelse fundet for sag " + caseId.ToString("D"));
                return false;
            }

            Entity account = _serviceClient.Retrieve(AccountLogicalName, accountReference.Id, new ColumnSet(AccountNumber, AccountPrimaryContact, AccountProperty, AccountLastAccountingDate));
            EntityReference contactRef = account.GetAttributeValue<EntityReference>(AccountPrimaryContact);
            EntityReference propertyRef = account.GetAttributeValue<EntityReference>(AccountProperty);
            if (contactRef == null || propertyRef == null)
            {
                _trace("Sag " + caseId.ToString("D") + " blev skippet, fordi konto mangler primær kontakt eller ejendom.");
                return false;
            }

            Entity contact = _serviceClient.Retrieve(ContactLogicalName, contactRef.Id, new ColumnSet(ContactCompanyId, ContactGovernmentId, ContactFullName, ContactAddress1, ContactPostalCode, ContactCity));
            Entity property = _serviceClient.Retrieve(PropertyLogicalName, propertyRef.Id, new ColumnSet(PropertyAddress));

            string caseNumber = ReadString(incident, IncidentCaseNumber);
            if (string.IsNullOrWhiteSpace(caseNumber))
                caseNumber = ReadString(incident, IncidentFallbackCaseNumber);

            candidate = new KontoAfslutArealSagerCandidate(
                caseId,
                caseNumber,
                incident.GetAttributeValue<DateTime?>(IncidentCreatedOn) ?? DateTime.MinValue,
                account.Id,
                ReadString(account, AccountNumber),
                contact.Id,
                ReadString(contact, ContactFullName),
                ReadString(contact, ContactGovernmentId),
                ReadString(contact, ContactCompanyId),
                ReadString(contact, ContactAddress1),
                ReadString(contact, ContactPostalCode),
                ReadString(contact, ContactCity),
                property.Id,
                ReadString(property, PropertyAddress),
                account.GetAttributeValue<DateTime?>(AccountLastAccountingDate));

            return true;
        }

        private EntityReference FindAccountReference(Guid caseId)
        {
            var query = new QueryExpression(ConnectionLogicalName)
            {
                ColumnSet = new ColumnSet(ConnectionRecord1Id, ConnectionRecord2Id, ConnectionRecord1Type, ConnectionRecord2Type),
                PageInfo = new PagingInfo { Count = 250, PageNumber = 1 }
            };
            var filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition(ConnectionRecord1Id, ConditionOperator.Equal, caseId);
            filter.AddCondition(ConnectionRecord2Id, ConditionOperator.Equal, caseId);
            query.Criteria.AddFilter(filter);

            foreach (var connection in RetrieveAll(query, "connections"))
            {
                var record1 = connection.GetAttributeValue<EntityReference>(ConnectionRecord1Id);
                var record2 = connection.GetAttributeValue<EntityReference>(ConnectionRecord2Id);
                if (record1 != null && string.Equals(record1.LogicalName, AccountLogicalName, StringComparison.OrdinalIgnoreCase))
                    return record1;
                if (record2 != null && string.Equals(record2.LogicalName, AccountLogicalName, StringComparison.OrdinalIgnoreCase))
                    return record2;
            }

            return null;
        }

        private void TraceNoMatchDiagnostics(Guid userId, Guid subjectId, DateTime followupThreshold, KontoAfslutArealSagerRequest request)
        {
            if (request == null)
            {
                return;
            }

            if (request.HasForcedCaseSelector)
            {
                _trace("Ingen incidents matchede force-selector. ForceIncidentId=" + SafeValue(request.ForceIncidentId) + ", ForceSagsnummer=" + SafeValue(request.ForceSagsnummer) + ", ForceKontonr=" + SafeValue(request.ForceKontonr));
                return;
            }

            if (request.HasForcedAccountSelector)
            {
                try
                {
                    Guid accountId = ResolveAccountId(request.ForceKontonr);
                    int connectedIncidentCount = FindIncidentIdsForAccount(accountId).Count();
                    _trace("Nul-hit diagnostik. ForceKontonr=" + SafeValue(request.ForceKontonr) + ", AccountConnectedIncidents=" + connectedIncidentCount + ".");
                }
                catch (Exception ex)
                {
                    _trace("Nul-hit diagnostik for ForceKontonr fejlede: " + ex.Message);
                }
                return;
            }

            int ownerCount = CountIncidents(q =>
            {
                q.Criteria.AddCondition(IncidentOwnerId, ConditionOperator.Equal, userId);
            });

            int ownerAndSubjectCount = CountIncidents(q =>
            {
                q.Criteria.AddCondition(IncidentOwnerId, ConditionOperator.Equal, userId);
                q.Criteria.AddCondition(IncidentSubjectId, ConditionOperator.Equal, subjectId);
            });

            int ownerSubjectAndFollowupCount = CountIncidents(q =>
            {
                q.Criteria.AddCondition(IncidentOwnerId, ConditionOperator.Equal, userId);
                q.Criteria.AddCondition(IncidentSubjectId, ConditionOperator.Equal, subjectId);
                q.Criteria.AddCondition(IncidentFollowupBy, ConditionOperator.LessEqual, followupThreshold);
            });

            _trace("Nul-hit diagnostik. ActiveForOwner=" + ownerCount + ", ActiveForOwnerAndSubject=" + ownerAndSubjectCount + ", ActiveForOwnerSubjectAndFollowup=" + ownerSubjectAndFollowupCount + ", ForceKontonr=" + SafeValue(request.ForceKontonr));
        }

        private int CountIncidents(Action<QueryExpression> configure)
        {
            var query = new QueryExpression(IncidentLogicalName)
            {
                ColumnSet = new ColumnSet(IncidentId),
                PageInfo = new PagingInfo { Count = 500, PageNumber = 1 }
            };
            query.Criteria.AddCondition(IncidentStateCode, ConditionOperator.Equal, 0);
            configure?.Invoke(query);
            return RetrieveAll(query, "incident-diagnostic").Count;
        }

        private List<Entity> RetrieveAll(QueryExpression query, string label)
        {
            var result = new List<Entity>();
            while (true)
            {
                EntityCollection batch = _serviceClient.RetrieveMultiple(query);
                result.AddRange(batch.Entities);
                _trace("RetrieveAll '" + label + "' fetched=" + batch.Entities.Count + " total=" + result.Count);
                if (!batch.MoreRecords)
                    break;
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = batch.PagingCookie;
            }
            return result;
        }

        private static string ReadString(Entity entity, string attributeName)
        {
            object value = entity.Contains(attributeName) ? entity[attributeName] : null;
            if (value == null) return string.Empty;
            if (value is AliasedValue alias && alias.Value != null) return alias.Value.ToString();
            return value.ToString();
        }

        private static string SafeValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "<tom>" : value;
        }

        private static string SafeEntityReferenceName(EntityReference reference)
        {
            if (reference == null)
            {
                return "<tom>";
            }

            if (!string.IsNullOrWhiteSpace(reference.Name))
            {
                return reference.Name;
            }

            return reference.Id.ToString("D");
        }

        private static string SafeDate(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm:ss") : "<tom>";
        }

        public void Dispose()
        {
            _serviceClient?.Dispose();
        }
    }
}
