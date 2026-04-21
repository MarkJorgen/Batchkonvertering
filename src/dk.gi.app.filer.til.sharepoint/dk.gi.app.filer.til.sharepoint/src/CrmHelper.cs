using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dk.gi.app.filer.til.sharepoint
{
    public class CrmHelper
    {
        public CrmHelper(string crmConnectionString, ILogger logger)
        {
            _crmConnectionString = crmConnectionString;
            _logger = logger;
        }

        private ILogger _logger;
        private string _crmConnectionString;
        private IOrganizationService _provider { get; set; }
        private DateTime _createdProvider;

        public IOrganizationService Provider
        {
            get
            {
                if (_provider == null || _createdProvider.AddHours(4) < DateTime.Now)
                {
                    CrmServiceClient conn = new CrmServiceClient(_crmConnectionString);
                    _provider = conn.OrganizationWebProxyClient;

                    _createdProvider = DateTime.Now;
                }
                return _provider;
            }
        }

        internal int GetTotal()
        {
            _logger.LogInformation("GetTotal");

            QueryExpression q = new QueryExpression("annotation");
            q.PageInfo = new PagingInfo() { Count = 3000, PageNumber = 1 };
            q.Criteria.AddCondition("filename", ConditionOperator.NotNull);
            q.AddLink("letter", "objectid", "activityid", JoinOperator.Inner);
            int total = this.RetrieveAll(q).Count;
            q.LinkEntities.RemoveAt(0);

            q.AddLink("task", "objectid", "activityid", JoinOperator.Inner);
            total += this.RetrieveAll(q).Count;
            return total;

        }

        public List<Entity> RetrieveAll(QueryExpression query)
        {
            _logger.LogInformation("RetrieveAll");

            List<Entity> list = new List<Entity>();
            EntityCollection result = null;
            query.PageInfo = new PagingInfo() { Count = 3000, PageNumber = 1 };
            do
            {
                result = this.Provider.RetrieveMultiple(query);
                if (result.Entities.Count > 0)
                    list.AddRange(result.Entities);

                query.PageInfo.PageNumber += 1;
                query.PageInfo.PagingCookie = result.PagingCookie;

            } while (result.MoreRecords);

            return list;
        }

        public Entity GetSPLocation(string entityName)
        {
            _logger.LogInformation("GetSPLocation");

            QueryExpression q = new QueryExpression("sharepointdocumentlocation");
            q.ColumnSet.AllColumns = true;
            q.Criteria.AddCondition("relativeurl", ConditionOperator.Equal, entityName);
            return this.Provider.RetrieveMultiple(q).Entities.FirstOrDefault();
        }

        public Entity GetSPLocationRecord(Guid id)
        {
            _logger.LogInformation("GetSPLocationRecord");

            QueryExpression q = new QueryExpression("sharepointdocumentlocation");
            q.ColumnSet.AddColumn("relativeurl");
            q.Criteria.AddCondition("regardingobjectid", ConditionOperator.Equal, id);
            return this.Provider.RetrieveMultiple(q).Entities.FirstOrDefault();
        }

        internal void DeleteMimeAttachment(Guid id)
        {
            _logger.LogInformation("DeleteMimeAttachment");

            this.Provider.Delete("activitymimeattachment", id);
        }

        public List<Entity> GetEmailAttachments(List<EntityReference> removedIds)
        {
            _logger.LogInformation("GetEmailAttachments");

            QueryExpression query = new QueryExpression("activitymimeattachment");
            query.PageInfo = new PagingInfo() { Count = 15, PageNumber = 1 };
            query.ColumnSet.AddColumns("filename", "body", "objectid");
            query.AddLink("email", "objectid", "activityid");
            query.LinkEntities[0].Columns.AddColumn("subject");
            query.LinkEntities[0].EntityAlias = "obj";
            query.Criteria.AddCondition("filename", ConditionOperator.NotNull);
            //query.Criteria.AddCondition("objectid", ConditionOperator.Equal, "2A744E5F-FB3B-E511-9431-0050568472BA");

            if (removedIds.Count > 0)
                query.Criteria.AddCondition("objectid", ConditionOperator.NotIn, removedIds.Select(x => x.Id.ToString()).ToArray());

            return this.Provider.RetrieveMultiple(query).Entities.ToList();
        }

        internal List<Entity> GetPDFNotesFromEjendom(List<EntityReference> removedIds)
        {
            _logger.LogInformation("GetPDFNotesFromEjendom");

            QueryExpression q = new QueryExpression("annotation");
            q.PageInfo = new PagingInfo() { Count = 15, PageNumber = 1 };
            q.ColumnSet.AddColumns("filename", "documentbody", "objectid", "notetext", "createdon");
            q.Criteria.AddCondition("filename", ConditionOperator.EndsWith, ".pdf");
            q.AddLink("ap_ejendom", "objectid", "ap_ejendomid", JoinOperator.Inner);
            if (removedIds.Count > 0)
                q.Criteria.AddCondition("objectid", ConditionOperator.NotIn, removedIds.Select(x => x.Id.ToString()).ToArray());

            q.LinkEntities[0].Columns.AddColumn("ap_samletadresse");
            q.LinkEntities[0].EntityAlias = "obj";
            var result = this.Provider.RetrieveMultiple(q).Entities.ToList();

            //Parse filename
            foreach (var item in result)
            {
                item.GetAttributeValue<EntityReference>("objectid").LogicalName = "ap_ejendom";
                item["obj.subject"] = item.GetAttributeValue<AliasedValue>("obj.ap_samletadresse");
                item["filename"] = $"{item.GetAttributeValue<DateTime>("createdon").ToString("yyyyMMdd")}_{item.GetAttributeValue<string>("filename")}";
            }

            return result;
        }

        public List<Entity> GetNotes(List<EntityReference> removedIds)
        {
            _logger.LogInformation("GetNotes");

            QueryExpression q = new QueryExpression("annotation");
            q.PageInfo = new PagingInfo() { Count = 15, PageNumber = 1 };
            q.ColumnSet.AddColumns("filename", "documentbody", "objectid", "notetext");
            q.Criteria.AddCondition("filename", ConditionOperator.NotNull);
            q.AddLink("letter", "objectid", "activityid", JoinOperator.Inner);
            if (removedIds.Count > 0)
                q.Criteria.AddCondition("objectid", ConditionOperator.NotIn, removedIds.Select(x => x.Id.ToString()).ToArray());

            q.LinkEntities[0].Columns.AddColumn("subject");
            q.LinkEntities[0].EntityAlias = "obj";
            var result = this.Provider.RetrieveMultiple(q).Entities.ToList();
            q.LinkEntities.RemoveAt(0);

            q.AddLink("email", "objectid", "activityid");
            q.LinkEntities[0].Columns.AddColumn("subject");
            q.LinkEntities[0].EntityAlias = "obj";
            result.AddRange(this.Provider.RetrieveMultiple(q).Entities.ToList());
            q.LinkEntities.RemoveAt(0);

            q.AddLink("phonecall", "objectid", "activityid", JoinOperator.Inner);
            q.LinkEntities[0].Columns.AddColumn("subject");
            q.LinkEntities[0].EntityAlias = "obj";
            result.AddRange(this.Provider.RetrieveMultiple(q).Entities.ToList());
            q.LinkEntities.RemoveAt(0);

            q.AddLink("task", "objectid", "activityid", JoinOperator.Inner);
            q.LinkEntities[0].Columns.AddColumn("subject");
            q.LinkEntities[0].EntityAlias = "obj";
            var list = this.Provider.RetrieveMultiple(q).Entities.ToList();

            //fix regarding bug entity
            foreach (var item in list)
                item.GetAttributeValue<EntityReference>("objectid").LogicalName = "task";

            result.AddRange(list);

            return result;
        }

        public void DeleteNote(Guid id)
        {
            _logger.LogInformation("DeleteNote");

            this.Provider.Delete("annotation", id);
        }

        public void RemoveFileNote(Guid id)
        {
            _logger.LogInformation("RemoveFileNote");

            Entity e = new Entity("annotation");
            e.Id = id;
            e["documentbody"] = null;
            e["filename"] = null;
            e["isdocument"] = false;
            e["filesize"] = null;
            this.Provider.Update(e);
        }

        public string GetSPSite()
        {
            _logger.LogInformation("GetSPSite");

            QueryExpression q = new QueryExpression("sharepointsite");
            q.ColumnSet.AllColumns = true;
            var entity = Provider?.RetrieveMultiple(q)?.Entities.FirstOrDefault();
            return entity != null && entity.Contains("absoluteurl") ? entity["absoluteurl"]?.ToString() : null;
        }

        internal Entity CreateLocation(Entity root, EntityReference record)
        {
            _logger.LogInformation("CreateLocation");

            Entity e = new Entity("sharepointdocumentlocation");
            e.Id = Guid.NewGuid();
            e["parentsiteorlocation"] = new EntityReference("sharepointdocumentlocation", root.Id);
            e["name"] = root["name"];
            e["locationtype"] = new OptionSetValue(0);
            e["servicetype"] = new OptionSetValue(0);
            e["regardingobjectid"] = record;
            e["sitecollectionid"] = new Guid("8d2a0df6-36d0-ea11-8128-000d3aac8f39");
            e["relativeurl"] = ParsePath(string.Concat(record.Name, "_", record.Id.ToString().Replace("-", "").ToUpper()));


            Provider.Create(e);

            return e;
        }
        private string ParsePath(string path)
        {
            _logger.LogInformation("ParsePath");

            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "").Replace("%", "").Replace("...", "").Replace("..", "").Replace("&", "").Replace("#", "").Replace("~", "").Replace("}", "").Trim();
        }

    }
}
