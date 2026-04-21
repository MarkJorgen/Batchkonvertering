/// <summary>
/// Koden her er den del af klassen GIConsoleApp (Partial)
/// 
/// Version: 2022 12 19
/// Sidste ændring: Changed Result pattern to be AppStatus and not AppStatus.StateCode
///
/// Det er primært i denne at du skal rette, her skal den primære aktuelle program kode lægges
/// </summary>

using System;
using System.IO;
using System.Messaging;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using dk.gi;
using dk.gi.crm;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using dk.gi.crm.models;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using dk.gi.crm.giproxy;
using System.Drawing;
using System.IdentityModel.Metadata;
using System.Text;
using System.Text.Json.Nodes;
using dk.gi.asbq;

//namespace dk.gi.app.console.template
namespace dk.gi.crm.app.faelles.cprvalidering
{
    /// <summary>
    /// Din kode skal tilføjes i Metoden Start her i dette objekt
    /// </summary>
    public partial class GIConsoleApp
    {
        /// <summary>
        /// Her udføres den egentlige behandling
        /// </summary>
        /// <returns>AppStatus</returns>
        private AppStatus CallBackFunction()
        {
            AppStatus result = new AppStatus();
            Trace.LogInformation("CallBackFunction blev kaldt");

            //// ****************************************
            //// Indtast tekst her
            //// -MODE=BATCH Hvis mode er BATCH så start validering
            //// ****************************************
            if (appConfig.Mode.ToUpper() == "BATCH")
            {
                result = ValiderStart();
            }

            Trace.LogInformation("CallBackFunction slut");
            return result;
        }

        /// <summary>
        /// Do the job
        /// </summary>
        /// <returns></returns>
        private AppStatus ValiderStart()
        {
            AppStatus result = new AppStatus();
            result.SetStatus = AppStatus.StateCode.OK;
 
            // Get setting for Azure Service Bus
            //string configPrefix = "Azure.Service.Bus.Queue.Crm.Indbakke.";
            //var asbqSettings = crmcontext.GetConfigSettingAll(configPrefix);
            //Trace.LogInformation($"Opretter AzureServiceBusQueueHttpContext");
            //asbq.AzureServiceBusQueueHttpContext asbqContext = new dk.gi.asbq.AzureServiceBusQueueHttpContext(configPrefix, asbqSettings);            

            // Get settings from application config file
            string tilladSendTilDigitalPost = appConfig["TilladSendTilDigitalPost"];
            string Ejer = appConfig["Ejer"];
            string KundeId = appConfig["KundeId"];
            string Sagstitel = appConfig["Sagstitel"];
            string EmneId = appConfig["EmneId"];
            string Emne = appConfig["Emne"];
            string Aktivitetskode = appConfig["Aktivitetskode"];
            string Filnavn = appConfig["Filnavn"];
            string Skabelonnavn = appConfig["Skabelonnavn"];
            //string WebserviceUrl = appConfig["WebserviceUrl"];
            //string CertificatFil = appConfig["CertificatFil"];
            //string eksekveringDLL = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //string sti = System.IO.Path.GetDirectoryName(eksekveringDLL);
            //byte[] bCertifikat = File.ReadAllBytes(sti + @"\" + CertificatFil);
            //string Certificat = Convert.ToBase64String(bCertifikat);

            HentNyeKunderRequest hentNyeKunderRequest = new HentNyeKunderRequest(crmcontext)
            {
                PEP = false
            };
            HentNyeKunderResponse hentNyeKunderResponse = hentNyeKunderRequest.Execute<HentNyeKunderResponse>();
            if (hentNyeKunderResponse.Status.IsOK() == true)
            {
                HentNyeKunderRequest hentNyeKunderPepRequest = new HentNyeKunderRequest(crmcontext)
                {
                    PEP = true
                };
                HentNyeKunderResponse hentNyeKunderPepResponse = hentNyeKunderPepRequest.Execute<HentNyeKunderResponse>();
                if (hentNyeKunderPepResponse.Status.IsOK() == true)
                {
                    hentNyeKunderResponse.NyeKunder.AddRange(hentNyeKunderPepResponse.NyeKunder);
                    foreach (NyKunde nyKunde in hentNyeKunderResponse.NyeKunder)
                    {
                        if (nyKunde.PEP.ToUpper() == "JA" || nyKunde.PEP.ToUpper() == "NEJ")
                        {
                            // Pass all settings to application handling this job
                            Trace.LogInformation($"Opret job for kunde:{nyKunde.Navn} {nyKunde.PEP} id {nyKunde.KundeId}");
                            JsonKeyValueList job = new JsonKeyValueList();
                            job.AddKeyValue("ContactId", nyKunde.KundeId.ToStringForCRM());
                            job.AddKeyValue("Ejer", Ejer);
                            job.AddKeyValue("KundeId", KundeId);
                            job.AddKeyValue("Sagstitel", Sagstitel);
                            job.AddKeyValue("EmneId", EmneId);
                            job.AddKeyValue("Emne", Emne);
                            job.AddKeyValue("Aktivitetskode", Aktivitetskode);
                            job.AddKeyValue("Filnavn", Filnavn);
                            job.AddKeyValue("Skabelonnavn", Skabelonnavn);
                            job.AddKeyValue("TilladSendTilDigitalPost", tilladSendTilDigitalPost);
                            //job.AddKeyValue("WebserviceUrl", WebserviceUrl);
                            //job.AddKeyValue("Certificat", Certificat);
                            //20230508 JMW Rettet til at bruge generisk rutine til service bus
                            //SendJobToServiceBusQueueTopic(asbqContext, asbqTopicName, asbqLabel, job);
                            dk.gi.asbq.jobqueue koe = new dk.gi.asbq.jobqueue(crmcontext);
                            // Config setting:Azure.Service.Bus.Queue.Label.CPRVALIDERING
                            if (koe.SendToQueue(job, dk.gi.asbq.AzureServiceBusQueueLabels.CPRVALIDERING) == false)
                            {
                                throw new Exception("CPRVALIDERING skrivning til kø i mislykkedes!");
                            }

                        }
                        else
                            Trace.LogWarning($"Undladt Kunde:{nyKunde.Navn} {nyKunde.PEP} id {nyKunde.KundeId}");
                    }
                }
                else
                    result.SetStatus = AppStatus.StateCode.AppExceptionInCode; // Din fejlkode som retuneres til OS -  lad den stå medmindre du ønsker et andet tal (bruges nedenfor til fejlhåndtering)
            }
            else
                result.SetStatus = AppStatus.StateCode.AppExceptionInCode; // Din fejlkode som retuneres til OS -  lad den stå medmindre du ønsker et andet tal (bruges nedenfor til fejlhåndtering)

            return result;
        }

        asbq.ServiceBusHttpClient msmqClient = null;
        private void SendJobToServiceBusQueueTopic(asbq.AzureServiceBusQueueHttpContext asbqContext, string QueueName, string msgLabel, JsonKeyValueList json)
        {
            Trace.LogInformation("SendJobToServiceBusQueueTopic start");
            if (msmqClient == null)
            {
                Trace.LogInformation($"SendJobToServiceBusQueueTopic Opretter ServiceBusHttpClient");
                msmqClient = new asbq.ServiceBusHttpClient(asbqContext);

            }
            msmqClient.SendMessage(QueueName, msgLabel, json.ToString());

            Trace.LogInformation("SendJobToServiceBusQueueTopic slut");
        }
    }
}