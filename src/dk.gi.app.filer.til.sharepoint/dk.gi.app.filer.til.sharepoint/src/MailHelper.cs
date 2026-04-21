using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.app.filer.til.sharepoint
{
    internal class MailHelper
    {
        /// <summary>
        /// Send en Email om status på kørsel af job
        /// </summary>
        /// 
        /// <param name="message"></param>
        /// <param name="emailModtagere"></param>
        /// <param name="crmServerName"></param>
        /// <param name="emailClientId"></param>
        /// <param name="emailClientSecret"></param>
        /// <param name="emailTenantid"></param>
        /// <param name="mailAfsenderMailAdressse"></param>
        internal static void SendEmail(ILogger logger, string message, string[] emailModtagere, string crmServerName, string emailClientId, string emailClientSecret, string emailTenantid, string mailAfsenderMailAdressse)
        {
            logger.LogInformation("SendEmail");

            try
            {
                if (emailModtagere.Length >= 1)
                {
                    logger.LogInformation("Sender email");

                    // Indholdet af mail
                    string subject = $"Programfejl App:{"dk.gi.app.filer.til.sharepoint"}, Miljø:{crmServerName}, Dato:{System.DateTime.Now.ToString("yyyy-MM-dd hh:mm")}";
                    string strbody = $"Der opstod fejl:{message}, og programmet kørte videre.";
                    // Opret context med email konfiguration som send mail med ovenstående indhold
                    dk.gi.email.EmailContext eContext = new dk.gi.email.EmailContext(emailClientId, emailClientSecret, emailTenantid, mailAfsenderMailAdressse);
                    //if (dk.gi.email.EmailClient.SendEmail(eContext, subject, strbody, emailModtagere) == false)
                    //    throw new Exception("SendEmail fejl fik ikke sendt E-mail");
                }
                else
                    throw new Exception("SendEmail fejl mangler modtagere");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
