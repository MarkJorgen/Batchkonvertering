using dk.gi.cpr.servicelink;
using dk.gi.crm;
using dk.gi.crm.data.bll;
using dk.gi.crm.managers;
using dk.gi.crm.managers.V2;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using dk.gi.email;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mail;
using System.Xml.Linq;
using static Aspose.Pdf.Operator;

namespace dk.gi.app.filer.til.sharepoint
{
    /// <summary>
    /// FilerTilSharePointRequest 
    /// </summary>
    public class FilerTilSharePointRequest : CrmRequest
    {
        public FilerTilSharePointRequest(CrmContext context) : base(context) { }

        /// <summary>
        /// CrmConnectionString
        /// </summary>
        [DataMember(IsRequired = true)]
        public string CrmConnectionString { get; set; }

        /// <summary>
        /// CrmServerName
        /// </summary>
        [DataMember(IsRequired = true)]
        public string CrmServerName { get; set; }

        /// <summary>
        /// EmailClientId
        /// </summary>
        [DataMember(IsRequired = true)]
        public string EmailClientId { get; set; }

        /// <summary>
        /// EmailClientSecret
        /// </summary>
        [DataMember(IsRequired = true)]
        public string EmailClientSecret { get; set; }

        /// <summary>
        /// EmailTenantid
        /// </summary>
        [DataMember(IsRequired = true)]
        public string EmailTenantid { get; set; }

        /// <summary>
        /// EmailAfsenderMailAdressse
        /// </summary>
        [DataMember(IsRequired = true)]
        public string EmailAfsenderMailAdressse { get; set; }

        /// <summary>
        /// EmailModtagere
        /// </summary>
        [DataMember(IsRequired = true)]
        public string[] EmailModtagere { get; set; }

        private string SiteUrl = null;
        CrmHelper crmHelper = null;
        SharePointHelper spHelper = null;
        string TenantId = null;
        string Clientid = null;
        string Clientsecret = null;

        /// <summary>
        /// Funktionen ExecuteRequest indeholder den kode der skal udføres.
        /// </summary>
        /// <returns>Et response som altid indeholder en Status på hvordan udførsel gik</returns>
        protected override IResponse ExecuteRequest()
        {
            // Ret GenericStrignResponse til dit eget response 
            CrmResponse result = new CrmResponse();

            try
            {
                this.Trace.LogInformation("Har opsat tracing, opretter et CRM data proxy objekt.");

                // Hent/valider config til Sharepoint
                SortedList<string, string> spSettings = this.localCrmContext.GetConfigSettingAll("Azure.");
                // Valider indlæste parametre
                this.TenantId = spSettings.Where(x => x.Key == "Azure.tenantId").Select(s => s.Value).FirstOrDefault();  // find nøgle: 
                if (string.IsNullOrEmpty(this.TenantId) == true)
                    throw new Exception($"Configurationssettings Azure.tenantId blev ikke fundet");

                this.Clientid = spSettings.Where(x => x.Key == "Azure.clientid").Select(s => s.Value).FirstOrDefault();  // find nøgle: 
                if (string.IsNullOrEmpty(this.Clientid) == true)
                    throw new Exception($"Configurationssettings Azure.clientid blev ikke fundet");

                this.Clientsecret = spSettings.Where(x => x.Key == "Azure.clientsecret").Select(s => s.Value).FirstOrDefault();  // find nøgle: 
                if (string.IsNullOrEmpty(this.Clientsecret) == true)
                    throw new Exception($"Configurationssettings Azure.clientsecret blev ikke fundet");

                //this.SiteUrl = $"https://{spSettings.Where(x => x.Key == "Azure.SharePoint.GIRootUrl").Select(s => s.Value).FirstOrDefault()}/";
                //if (string.IsNullOrEmpty(this.SiteUrl) == true)
                //    throw new Exception($"Configurationssettings Azure.SharePoint.GIRootUrl blev ikke fundet");

                crmHelper = new CrmHelper(this.CrmConnectionString, Trace);

                this.SiteUrl = crmHelper.GetSPSite();

                this.Trace.LogInformation($"{this.SiteUrl}");

                spHelper = new SharePointHelper(this.SiteUrl, this.TenantId, this.Clientid, this.Clientsecret, this.Trace,
                    this.EmailModtagere, this.CrmServerName, this.EmailClientId, this.EmailClientSecret, this.EmailTenantid, this.EmailAfsenderMailAdressse);

                List<EntityReference> removedIds = new List<EntityReference>();
                List<Entity> notes = new List<Entity>();
                List<Entity> mimeAttachments = new List<Entity>();
                int files = 0;

                do
                {
                    try
                    {
                        this.Trace.LogInformation("Fetching PDF from Ejendom...");

                        notes = crmHelper.GetPDFNotesFromEjendom(removedIds);
                        files = notes.Count;

                        files = notes.Count;
                        MigrateFiles(notes, true, ref removedIds, true);

                        this.Trace.LogInformation("Fetching Notes...");

                        notes = crmHelper.GetNotes(removedIds);
                        files += notes.Count;

                        files += notes.Count;
                        MigrateFiles(notes, true, ref removedIds, false);

                        this.Trace.LogInformation("Fetching MimeAttachments...");

                        mimeAttachments = crmHelper.GetEmailAttachments(removedIds);
                        this.Trace.LogInformation($"Fetching MimeAttachments got {mimeAttachments.Count}...");

                        MigrateFiles(mimeAttachments, false, ref removedIds, false);
                    }
                    catch (Exception ex)
                    {
                        this.Trace.LogError($"Error - Fetching PDF from Ejendom: {ex.Message}");

                        MailHelper.SendEmail(this.Trace, ex.Message, this.EmailModtagere, this.CrmServerName, this.EmailClientId, this.EmailClientSecret, this.EmailTenantid, this.EmailAfsenderMailAdressse);

                        Thread.Sleep(5000);
                        crmHelper = new CrmHelper(this.CrmConnectionString, Trace);
                        spHelper = new SharePointHelper(this.SiteUrl, this.TenantId, this.Clientid, this.Clientsecret, this.Trace,
                            this.EmailModtagere, this.CrmServerName, this.EmailClientId, this.EmailClientSecret, this.EmailTenantid, this.EmailAfsenderMailAdressse);
                    }

                } while (files > 0 || mimeAttachments.Count > 0);
            }
            catch (Exception exception)
            {
                string fejl = exception.Message;
                this.Trace.LogError(fejl);
                result.Status.AppendError(fejl);
            }

            if (result.Status.IsOK())
            {
                // Information to trace, code completed this method without exceptions
                Trace.LogInformation($"Request {GetType().Name} blev gennemført");
            }

            return result;
        }

        public void MigrateFiles(List<Entity> files, bool isNote, ref List<EntityReference> removedIds, bool bookSkipExistingFile)
        {
            this.Trace.LogInformation("MigrateFiles");

            Hashtable rootSites = new Hashtable();
            Hashtable folderRecords = new Hashtable();
            string attributeBody = "body";

            if (isNote)
                attributeBody = "documentbody";

            string arbejderPaa = string.Empty;

            foreach (var file in files)
            {
                EntityReference parentRecord = null;

                try
                {
                    string fileName = file.GetAttributeValue<string>("filename");

                    if (file.Contains(attributeBody))
                    {
                        arbejderPaa = $"Arbejder på filnavn {fileName} entitet {file.LogicalName} id {file.Id}";
                        this.Trace.LogInformation(arbejderPaa);

                        var content = Convert.FromBase64String(file.GetAttributeValue<string>(attributeBody));
                        parentRecord = file.GetAttributeValue<EntityReference>("objectid");

                        if (!isNote && string.IsNullOrEmpty(parentRecord.LogicalName))
                            parentRecord.LogicalName = "email";

                        parentRecord.Name = file.GetAttributeValue<AliasedValue>("obj.subject")?.Value?.ToString();

                        if (string.IsNullOrEmpty(parentRecord.Name))
                            parentRecord.Name = "EMPTY-SUBJECT";

                        this.Trace.LogInformation($"ParentRecord: {parentRecord?.LogicalName} {parentRecord?.Id}  {parentRecord?.Name}");

                        Entity parentSite = null;
                        Entity spFolder = null;

                        if (rootSites.ContainsKey(parentRecord.LogicalName))
                            parentSite = rootSites[parentRecord.LogicalName] as Entity;
                        else
                        {
                            parentSite = crmHelper.GetSPLocation(parentRecord.LogicalName);
                            rootSites[parentRecord.LogicalName] = parentSite;
                        }

                        this.Trace.LogInformation($"parentSite: {dk.gi.crm.Extending.EntityXml(parentSite)}");

                        if (folderRecords.ContainsKey(parentRecord.Id))
                        {
                            spFolder = folderRecords[parentRecord.Id] as Entity;
                            this.Trace.LogInformation($"spFolder parentRecord.Id: {dk.gi.crm.Extending.EntityXml(spFolder)}");
                        }
                        else
                        {
                            spFolder = crmHelper.GetSPLocationRecord(parentRecord.Id);
                            this.Trace.LogInformation($"spFolder GetSPLocationRecord: {dk.gi.crm.Extending.EntityXml(spFolder)}");

                            if (spFolder == null)
                            {
                                spFolder = crmHelper.CreateLocation(parentSite, parentRecord);
                                this.Trace.LogInformation($"spFolder CreateLocation: {dk.gi.crm.Extending.EntityXml(spFolder)}");
                            }

                            spHelper.CreateFolder(parentSite.GetAttributeValue<string>("relativeurl"), spFolder.GetAttributeValue<string>("relativeurl"));

                            folderRecords[parentRecord.Id] = spFolder;
                        }

                        fileName = TrimIllegalChars(fileName.Trim());

                        bool ok = spHelper.UploadFile(false, parentSite.GetAttributeValue<string>("relativeurl"),
                                                        spFolder.GetAttributeValue<string>("relativeurl"),
                                                        fileName, content, bookSkipExistingFile);

                        if (!ok)
                        {
                            if (fileName.Length > 50)
                                fileName = fileName.Substring(0, 50);

                            this.Trace.LogInformation($"Ikke ok prøver igen med: {fileName}");

                            spHelper.UploadFile(true, parentSite.GetAttributeValue<string>("relativeurl"),
                                                            spFolder.GetAttributeValue<string>("relativeurl"),
                                                            fileName, content, bookSkipExistingFile);
                        }

                        if (isNote)
                        {
                            if (file.Contains("notetext"))
                                crmHelper.RemoveFileNote(file.Id);
                            else
                                crmHelper.DeleteNote(file.Id);
                        }
                        else
                            crmHelper.DeleteMimeAttachment(file.Id);

                    }
                    else if (isNote)
                    {
                        this.Trace.LogInformation($"RemoveFileNote på {fileName}");
                        crmHelper.RemoveFileNote(file.Id);
                    }
                    else
                    {
                        this.Trace.LogInformation($"DeleteMimeAttachment på {fileName}");
                        crmHelper.DeleteMimeAttachment(file.Id);
                    }
                }
                catch (Exception e1)
                {
                    if (parentRecord != null)
                        removedIds.Add(parentRecord);

                    this.Trace.LogError($"Error - {arbejderPaa} {e1.ToString()}");

                    MailHelper.SendEmail(this.Trace, e1.Message, this.EmailModtagere, this.CrmServerName, this.EmailClientId, this.EmailClientSecret, this.EmailTenantid, this.EmailAfsenderMailAdressse);

                    Thread.Sleep(1000);
                    crmHelper = new CrmHelper(this.CrmConnectionString, Trace);
                    spHelper = new SharePointHelper(this.SiteUrl, this.TenantId, this.Clientid, this.Clientsecret, this.Trace,
                        this.EmailModtagere, this.CrmServerName, this.EmailClientId, this.EmailClientSecret, this.EmailTenantid, this.EmailAfsenderMailAdressse);
                }
            }
        }

        public string TrimIllegalChars(string value)
        {
            char[] IllegalChars = "*:\"<>?|#%/\\\r\n\t".ToCharArray();

            foreach (char c in IllegalChars)
            {
                value = value.Replace(c, '_');
            }

            return value;
        }
    }
}
