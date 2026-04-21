using System;
using System.Collections.Generic;
using Serilog;
using Newtonsoft.Json;
using System.IO;
using System.Messaging;
using System.Configuration;
using dk.gi.crm.request;

namespace dk.gi.crm.app.konto.slettetKontoKorrektion
{
    /// <summary>
    /// Alle konti, der har ap_statusframapper = slettet.
    /// Hvis § 18 saldo(ap_18saldo) er forskellig fra 0,00 eller § 18 B/ 63 A(ap_bindingspligt) forskellig fra 0,00 
    /// så opret et regnskab relateret til kontoen med startdato = ap_sidsteregnskabsdato minus én dag, og slutdato = ap_sidsteregnskabsdato
    /// Regnskabsårsag skal være = 20 Regulering.
    /// ap_pgrf18henst skal være § 18 saldo fra konto(ap_18saldo) * minus én.
    /// ap_pgrf18bhenst skal være § 18 B/63a(ap_bindingpligt) * minus én
    ///
    /// Dette skulle gerne føre til at de to saldi på kontoen går i nul.
    ///
    /// </summary>
    /// <remarks>
    /// Oprettet af RCL, ALFAPEOPLE den 2018-03-02
    /// </remarks>

    class Program
    {

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.RollingFile(ConfigurationManager.AppSettings["log"].ToString())
                            .CreateLogger();

            Log.Information("Starter dk.gi.crm.app.konto.slettetKontoKorrektion");

            string connection = "Url = " + args[0].Replace("-CrmServer=", ""); // ConfigurationManager.AppSettings["connection"].ToString();


            try
            {
                SlettetKontoKorrektionRequest slettetKontoKorrektionRequest = new SlettetKontoKorrektionRequest(connection, new dk.faelles.gisporing.GISporingDynamicAdapter(Log.Logger));
                slettetKontoKorrektionRequest.Execute();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Fejl");
            }


            Log.Information($"Afslutter dk.gi.crm.app.konto.slettetKontoKorrektion");
        }

    }
}
