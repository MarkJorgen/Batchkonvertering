/// <summary>
/// Koden her er den del af klassen GIConsoleApp (Partial)
///
/// Version: 2021 09 09
/// Sidste ændring: Tilpasset til Extensions Logging
///
/// Når du opretter en ny applikation er det tanken at denne Program_App_Plus_MSMQ_template.cs kopieres til din app Program_App_Plus_MSMQ.cs (Og sættes til Compile)
/// Når du opgraderer NuGet Pakken "dk.gi.app.console.template.GiNugetSrc", så gentages kopieringen herover, men nu med overskriv (Har du rettet, så red dine rettelser først)
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;

using dk.gi;
using dk.gi.app;

//namespace dk.gi.app.console.template
namespace dk.gi.crm.app.konto.slettetKontoKorrektion
{
    public partial class GIConsoleApp
    {
        /// <summary>
        /// Hent den Message fra MSMQ Indbakke som blev angivet med id som parameter til programmet
        /// </summary>
        /// <returns></returns>
        internal System.Messaging.Message HentMSMQFraIndkomneKoe(string msgid)
        {
//            if (string.IsNullOrEmpty(msgid) == false)
//           {
//                //// Hent MSMQ ID fra app config
//                Trace.LogInformation($"GIConsoleApp.HentMSMQFraIndkomneKoe: {appConfig.msgID}");
//                dk.gi.msmq.GImsmqMessageFilter filter = new dk.gi.msmq.GImsmqMessageFilter
//                {
//                    KoeNavn = dk.gi.msmq.GImsmqKoe.MeddelelsesKoeIndkomne,
//                    MessageId = appConfig.msgID
//                };
//
//                // Initier objekt som kan hjælpe med håndtering af kald til MSMQ
//                dk.gi.msmq.GImsmqSimple meddelelsesKoe = new dk.gi.msmq.GImsmqSimple( appConfig.GIMsmqServer);
//                // Hent Liste fra MSMQ
//                List<System.Messaging.Message> msgList = meddelelsesKoe.GetFilteredList(filter);
//                Trace.LogInformation($"Liste hentet antal: {msgList.Count()}");
//                return msgList.First();
//            }
//            else
//                Trace.LogWarning($"Kunne ikke flytte til afsluttet kø, msgid ikke udfyldt {appConfig.GIMsmqServer}");
            return null;
        }

        /// <summary>
        /// Flyt den behandlede msmq til afsluttet kø
        /// </summary>
        internal void FlytMessageToAfsluttet(string msgid)
        {
            if (string.IsNullOrEmpty(msgid) == false)
            {
//                Trace.LogInformation($"Flytter id:{msgid} til afviklet kø på:{appConfig.GIMsmqServer}");
//                dk.gi.msmq.GImsmqSimple meddelelsesKoe = new dk.gi.msmq.GImsmqSimple( appConfig.GIMsmqServer);
//                meddelelsesKoe.MoveMessageFromQueToQue(msgid, dk.gi.msmq.GImsmqKoe.MeddelelsesKoeIndkomne, dk.gi.msmq.GImsmqKoe.MeddelelsesKoeAfviklet);
            }
//            else
//                Trace.LogWarning($"Kunne ikke flytte til afsluttet kø, msgid ikke udfyldt {appConfig.GIMsmqServer}");
        }

        /// <summary>
        /// Flyt den behandlede msmq til fejl kø
        /// </summary>
        internal void FlytMessageToFejl(string msgid)
        {
            if (string.IsNullOrEmpty(msgid) == false)
            {
//                Trace.LogInformation($"Flytter id:{msgid} til fejl kø på:{appConfig.GIMsmqServer}");
//                dk.gi.msmq.GImsmqSimple meddelelsesKoe = new dk.gi.msmq.GImsmqSimple( appConfig.GIMsmqServer);
//                meddelelsesKoe.MoveMessageFromQueToQue(msgid, dk.gi.msmq.GImsmqKoe.MeddelelsesKoeIndkomne, dk.gi.msmq.GImsmqKoe.MeddelelsesKoeFejl);
            }
//            else
//                Trace.LogWarning($"Kunne ikke flytte til fejl kø, msgid ikke udfyldt {appConfig.GIMsmqServer}");
        }
    }
}