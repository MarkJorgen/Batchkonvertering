using System;
using System.Collections.Generic;
using System.Linq;
using dk.gi.app.contact.registrering.optaelling.Application.Abstractions;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace dk.gi.app.contact.registrering.optaelling.Infrastructure.Crm
{
    public sealed class ContactRegistreringDataverseClient : IContactRegistreringWorkflowClient
    {
        private const string ContactLogicalName = "contact";
        private const string EjerRegistreringLogicalName = "ap_ejerregistrering";
        private const string RegistreringLogicalName = "ap_registrering";
        private const string TreklipLogicalName = "ap_treklip";

        private const string ContactId = "contactid";
        private const string SamletNavnAdresse = "ap_samletnavnadresse";
        private const string Type4Registreringer = "ap_type4registreringer";
        private const string Type5Registreringer = "ap_type5registreringer";
        private const string UdlobneType4Registreringer = "ap_udlobnetype4registreringer";
        private const string UdlobneType5Registreringer = "ap_udlobnetype5registreringer";

        private readonly ServiceClient _serviceClient;
        private readonly IOrganizationService _service;
        private bool _disposed;

        public ContactRegistreringDataverseClient(string connectionString)
        {
            _serviceClient = new ServiceClient(connectionString);
            if (_serviceClient == null)
            {
                throw new InvalidOperationException("Failed to connect to Dataverse. ServiceClient instance was null.");
            }

            if (_serviceClient.IsReady == false)
            {
                string lastError = _serviceClient.LastError ?? string.Empty;
                string lastException = _serviceClient.LastException != null ? _serviceClient.LastException.ToString() : string.Empty;
                throw new InvalidOperationException("Failed to connect to Dataverse. LastError='" + lastError + "'. LastException='" + lastException + "'.");
            }

            _service = (IOrganizationService)_serviceClient;
        }

        public ContactRegistreringExecutionSummary VerifyConnection()
        {
            return ContactRegistreringOperationResult.Success(
                "CRM-forbindelse valideret. Der blev ikke udført workflow-kald i VERIFYCRM-mode.",
                "local dataverse sdk");
        }

        public ContactRegistreringExecutionSummary CloseExpiredTreklipOwnerRegistrations()
        {
            try
            {
                int updated = 0;
                foreach (Guid ownerRegistrationId in GetExpiredTreklipOwnerRegistrationIds())
                {
                    var request = new SetStateRequest
                    {
                        EntityMoniker = new EntityReference(EjerRegistreringLogicalName, ownerRegistrationId),
                        State = new OptionSetValue(1), // inactive
                        Status = new OptionSetValue(2)
                    };

                    _service.Execute(request);
                    updated++;
                }

                return ContactRegistreringOperationResult.Success(
                    "Lukkede afsluttede treklip/ejerregistreringer: " + updated,
                    "local dataverse sdk");
            }
            catch (Exception ex)
            {
                return ContactRegistreringOperationResult.Failure(
                    "Lukning af afsluttede treklip/ejerregistreringer kastede exception: " + ex.Message,
                    ex.GetType().FullName);
            }
        }

        public ContactRegistreringExecutionSummary CreateJobsForContactRegistrerings(Guid? registreringId)
        {
            try
            {
                IReadOnlyCollection<Guid> contactIds = registreringId.HasValue
                    ? new[] { registreringId.Value }
                    : GetAllContactIdsWithRegistrerings();

                int updated = 0;
                int unchanged = 0;
                var failures = new List<string>();

                foreach (Guid contactId in contactIds)
                {
                    try
                    {
                        if (UpdateNumberOfRegistreringForContact(contactId))
                        {
                            updated++;
                        }
                        else
                        {
                            unchanged++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failures.Add(contactId + ": " + ex.Message);
                    }
                }

                if (failures.Count > 0)
                {
                    return ContactRegistreringOperationResult.Failure(
                        "Kontaktregistreringsflow gennemført med fejl. Updated=" + updated + ", Unchanged=" + unchanged + ", Failures=" + string.Join(" | ", failures),
                        "local dataverse sdk");
                }

                return ContactRegistreringOperationResult.Success(
                    "Kontaktregistreringsflow gennemført. Updated=" + updated + ", Unchanged=" + unchanged,
                    "local dataverse sdk");
            }
            catch (Exception ex)
            {
                return ContactRegistreringOperationResult.Failure(
                    "Dannelse af kontaktjobs kastede exception: " + ex.Message,
                    ex.GetType().FullName);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _serviceClient.Dispose();
            _disposed = true;
        }

        private IReadOnlyCollection<Guid> GetAllContactIdsWithRegistrerings()
        {
            var query = new QueryExpression(ContactLogicalName)
            {
                ColumnSet = new ColumnSet(ContactId),
                PageInfo = new PagingInfo
                {
                    Count = 1000,
                    PageNumber = 1
                }
            };

            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 1);
            query.AddLink(EjerRegistreringLogicalName, ContactId, "ap_ejerid")
                .LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 1);

            return RetrieveAll(query)
                .Select(x => x.Id)
                .Distinct()
                .ToList();
        }

        private bool UpdateNumberOfRegistreringForContact(Guid contactId)
        {
            Entity currentContact = ReadContact(contactId);
            if (currentContact == null)
            {
                throw new InvalidOperationException("Kontaktperson med id kunne ikke læses: " + contactId);
            }

            var counts = ReadRegistreringCounts(contactId);
            Entity update = new Entity(ContactLogicalName) { Id = contactId };

            update[Type4Registreringer] = counts.Type4Count;
            update[Type5Registreringer] = counts.Type5Count;
            update[UdlobneType4Registreringer] = counts.Type4CountUdl == 0 ? (int?)null : counts.Type4CountUdl;
            update[UdlobneType5Registreringer] = counts.Type5CountUdl == 0 ? (int?)null : counts.Type5CountUdl;

            if (IsRegistreringerChanged(currentContact, counts) == false)
            {
                return false;
            }

            _service.Update(update);
            return true;
        }

        private Entity ReadContact(Guid contactId)
        {
            return _service.Retrieve(
                ContactLogicalName,
                contactId,
                new ColumnSet(ContactId, SamletNavnAdresse, Type4Registreringer, Type5Registreringer, UdlobneType4Registreringer, UdlobneType5Registreringer));
        }

        private RegistreringCounts ReadRegistreringCounts(Guid contactId)
        {
            return new RegistreringCounts(
                type4Count: CountRegistreringer(contactId, 1, 1),
                type5Count: CountRegistreringer(contactId, 2, 1),
                type4CountUdl: CountRegistreringer(contactId, 1, 3),
                type5CountUdl: CountRegistreringer(contactId, 2, 3));
        }

        private int CountRegistreringer(Guid contactId, int registreringType, int registreringStatus)
        {
            var query = new QueryExpression(EjerRegistreringLogicalName)
            {
                ColumnSet = new ColumnSet("ap_ejerregistreringid"),
                Distinct = false,
                PageInfo = new PagingInfo
                {
                    Count = 50,
                    PageNumber = 1
                }
            };

            query.Criteria.FilterOperator = LogicalOperator.And;
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 1);
            query.Criteria.AddCondition("ap_ejerid", ConditionOperator.Equal, contactId);

            LinkEntity registreringLink = query.AddLink(RegistreringLogicalName, "ap_registreringid", "ap_registreringid");
            registreringLink.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            registreringLink.LinkCriteria.AddCondition("statuscode", ConditionOperator.Equal, 1);
            registreringLink.LinkCriteria.AddCondition("ap_datoforbortfald", ConditionOperator.Null);
            registreringLink.LinkCriteria.AddCondition("ap_type", ConditionOperator.Equal, registreringType);
            registreringLink.LinkCriteria.AddCondition("ap_status", ConditionOperator.Equal, registreringStatus);

            return RetrieveAll(query).Count;
        }

        private bool IsRegistreringerChanged(Entity currentContact, RegistreringCounts counts)
        {
            return HasChanged(currentContact, Type4Registreringer, counts.Type4Count)
                || HasChanged(currentContact, Type5Registreringer, counts.Type5Count)
                || HasChanged(currentContact, UdlobneType4Registreringer, counts.Type4CountUdl)
                || HasChanged(currentContact, UdlobneType5Registreringer, counts.Type5CountUdl);
        }

        private static bool HasChanged(Entity currentContact, string attributeName, int newValue)
        {
            int? existingValue = currentContact.Contains(attributeName)
                ? currentContact.GetAttributeValue<int?>(attributeName)
                : null;

            if (existingValue.HasValue)
            {
                return existingValue.Value != newValue;
            }

            return newValue != 0;
        }

        private IReadOnlyCollection<Guid> GetExpiredTreklipOwnerRegistrationIds()
        {
            var query = new QueryExpression(TreklipLogicalName)
            {
                Distinct = true,
                PageInfo = new PagingInfo
                {
                    Count = 1000,
                    PageNumber = 1
                }
            };

            query.Criteria.AddCondition("ap_status", ConditionOperator.Equal, 5);

            LinkEntity registreringLink = query.AddLink(RegistreringLogicalName, "ap_treklipid", "ap_treklipid");
            registreringLink.LinkCriteria.AddCondition("ap_status", ConditionOperator.Equal, 2);

            LinkEntity ownerLink = registreringLink.AddLink(EjerRegistreringLogicalName, "ap_registreringid", "ap_registreringid");
            ownerLink.Columns.AddColumns("ap_ejerregistreringid");
            ownerLink.EntityAlias = "ap_ejerregistrering";
            ownerLink.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            return RetrieveAll(query)
                .Select(entity => GetAliasedGuid(entity, "ap_ejerregistrering.ap_ejerregistreringid"))
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();
        }

        private List<Entity> RetrieveAll(QueryExpression query)
        {
            var entities = new List<Entity>();
            query.PageInfo = query.PageInfo ?? new PagingInfo { Count = 5000, PageNumber = 1 };

            while (true)
            {
                EntityCollection page = _service.RetrieveMultiple(query);
                entities.AddRange(page.Entities);

                if (page.MoreRecords == false)
                {
                    break;
                }

                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = page.PagingCookie;
            }

            return entities;
        }

        private static Guid GetAliasedGuid(Entity entity, string aliasName)
        {
            if (entity == null || entity.Contains(aliasName) == false)
            {
                return Guid.Empty;
            }

            var aliased = entity[aliasName] as AliasedValue;
            if (aliased?.Value is Guid guid)
            {
                return guid;
            }

            return Guid.Empty;
        }

        private readonly struct RegistreringCounts
        {
            public RegistreringCounts(int type4Count, int type5Count, int type4CountUdl, int type5CountUdl)
            {
                Type4Count = type4Count;
                Type5Count = type5Count;
                Type4CountUdl = type4CountUdl;
                Type5CountUdl = type5CountUdl;
            }

            public int Type4Count { get; }
            public int Type5Count { get; }
            public int Type4CountUdl { get; }
            public int Type5CountUdl { get; }
        }
    }
}
