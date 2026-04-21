using Azure.Identity;
using dk.gi.email;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dk.gi.app.filer.til.sharepoint
{
    public class SharePointHelper
    {
        private string ClientId;
        private string TenantId;
        private string ClientSecret;
        private Site RootSite;
        private List<Drive> Drives;
        private ILogger _logger;
        private string[] EmailModtagere;
        private string CrmServerName;
        private string EmailClientId;
        private string EmailClientSecret;
        private string EmailTenantid;
        private string EmailAfsenderMailAdressse;

        private GraphServiceClient _graphSvcClient;
        public GraphServiceClient GraphSvcClient
        {
            get
            {
                if (_graphSvcClient == null)

                {
                    var scopes = new[] { "https://graph.microsoft.com/.default" };
                    var options = new TokenCredentialOptions
                    {
                        AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                    };

                    ClientSecretCredential authProvider = new ClientSecretCredential(TenantId, ClientId, ClientSecret, options);
                    _graphSvcClient = new GraphServiceClient(authProvider, scopes);
                }
                return _graphSvcClient;
            }
        }

        public SharePointHelper(string siteUrl, string tenantId, string clientId, string clientSecret, ILogger logger,
            string[] mailModtagere, string crmServerName, string emailClientId, string emailClientSecret, string emailTenantid, string emailAfsenderMailAdressse)
        {
            _logger = logger;

            _logger.LogInformation("SharePointHelper: Ctor");

            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.TenantId = tenantId;
            this.EmailModtagere = mailModtagere;
            this.CrmServerName = crmServerName;
            this.EmailClientId = emailClientId;
            this.EmailClientSecret = emailClientSecret;
            this.EmailTenantid = emailTenantid;
            this.EmailAfsenderMailAdressse = emailAfsenderMailAdressse;

            var sites = this.GraphSvcClient.Sites.GetAsync().Result;
            this.RootSite = sites.Value.Where(a => a.WebUrl.ToLower() == siteUrl.ToLower()).FirstOrDefault();

            if (this.RootSite == null)
                throw new Exception($"Root Site not found! Url:{siteUrl}");

            this.Drives = this.GraphSvcClient.Sites[this.RootSite.Id].Drives.GetAsync().Result.Value;

            if (this.Drives?.Count == 0)
                throw new Exception($"Drives not found! Url:{siteUrl}");
        }

        internal void CreateFolder(string entityName, string location)
        {
            _logger.LogInformation($"CreateFolder: {entityName}, {location}");

            Drive drive = this.Drives.Where(d => d.WebUrl.EndsWith(entityName)).FirstOrDefault();

            if (drive == null)
                throw new Exception($"Drive not found! Location:{entityName}");

            int count = 1;
            while (count <= 3)
            {
                count++;

                try
                {
                    DriveItem folder = new DriveItem
                    {
                        Name = location,
                        Folder = new Folder(),
                    };

                    //Create Folder
                    var rootFolder = this.GraphSvcClient.Drives[drive.Id].Root.GetAsync().Result;
                    var newFolder = this.GraphSvcClient.Drives[drive.Id]
                        .Items[rootFolder.Id]
                        .Children
                        .PostAsync(folder).Result;

                    break;
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message == "Name already exists")
                    {
                        // _logger.LogInformation($"Error - CreateFolder - folder exists. Folder: {location}");

                        break;
                    }
                    else
                    {
                        _logger.LogError($"Error - CreateFolder. Folder: {location}");

                        MailHelper.SendEmail(_logger, $"Fejl - CreateFolder. Folder: {location} {ex.Message} | {(ex.InnerException?.Message ?? "")}",
                            this.EmailModtagere, this.CrmServerName, this.EmailClientId, this.EmailClientSecret, this.EmailTenantid, this.EmailAfsenderMailAdressse);

                        Thread.Sleep(5000);
                        if (count > 3)
                            throw ex;
                    }
                }
            }
        }

        public bool UploadFile(bool reportError, string entityName, string location, string name, byte[] content, bool bookSkipExistingFile)
        {
            _logger.LogInformation($"UploadFile: {reportError}, {entityName}, {location}, {name}, {content.Length}, {bookSkipExistingFile}");

            int count = 0;
            int indexName = 0;

            var uploadSessionBody = new CreateUploadSessionPostRequestBody
            {
                Item = new DriveItemUploadableProperties
                {
                    AdditionalData = new Dictionary<string, object> { { "@microsoft.graph.conflictBehavior", "fail" } },
                }
            };

            Drive drive = this.Drives.Where(d => d.WebUrl.EndsWith(entityName)).FirstOrDefault();

            while (count <= 3)
            {
                count++;
                try
                {
                    name = (indexName == 0) ? name : $"{indexName} - {name}";

                    var session = this.GraphSvcClient.Drives[drive.Id]
                        .Root
                        .ItemWithPath($"{location}/{name}")
                        .CreateUploadSession
                        .PostAsync(uploadSessionBody).Result;


                    using (MemoryStream mem = new MemoryStream(content))
                    {

                        int maxChunkSize = 320 * 1024;
                        var fileUploadTask = new LargeFileUploadTask<DriveItem>(session, mem, maxChunkSize);

                        IProgress<long> progress = new Progress<long>(prog =>
                        {
                            Console.WriteLine($"Uploaded {prog}");
                        });


                        var uploadResult = fileUploadTask.UploadAsync(progress).Result;

                        if (!uploadResult.UploadSucceeded)
                            throw new Exception($"Upload failed. File: {location}/{name}");
                    }
                    break;
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("name already exists"))
                    {
                        if (bookSkipExistingFile)
                        {
                            _logger.LogInformation($"UploadFile - File exists and SkipExistingFile is true, so skip file. File: {location}/{name}");

                            break;
                        }
                        else
                        {
                            _logger.LogError($"Error - UploadFile II: {ex.Message} | {(ex.InnerException?.Message ?? "")}");

                            MailHelper.SendEmail(_logger, $"Fejl - UploadFile: name already exists. Folder: {location} {ex.Message} | {(ex.InnerException?.Message ?? "")}", this.EmailModtagere, this.CrmServerName, this.EmailClientId, this.EmailClientSecret, this.EmailTenantid, this.EmailAfsenderMailAdressse);
                        }

                        indexName++;
                        count--;
                    }
                    else
                    {
                        if (reportError)
                        {
                            Thread.Sleep(5000);

                            if (count > 3)
                            {
                                _logger.LogError($"Error - UploadFile I: {ex.Message} | {(ex.InnerException?.Message ?? "")}");

                                MailHelper.SendEmail(_logger, $"Fejl - UploadFile. Folder: {location} {ex.Message} | {(ex.InnerException?.Message ?? "")}", this.EmailModtagere, this.CrmServerName, this.EmailClientId, this.EmailClientSecret, this.EmailTenantid, this.EmailAfsenderMailAdressse);

                                throw ex;
                            }
                        }
                        else
                        {
                            if (count > 3)
                                return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}