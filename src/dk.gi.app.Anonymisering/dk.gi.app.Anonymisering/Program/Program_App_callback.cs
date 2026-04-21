/// <summary>
/// Koden her er den del af klassen GIConsoleApp (Partial)
/// 
/// Version: 2022 12 19
/// Sidste ændring: Changed Result pattern to be AppStatus and not AppStatus.StateCode
///
/// Det er primært i denne at du skal rette, her skal den primære aktuelle program kode lægges
/// </summary>

using System;
using Microsoft.Extensions.Logging;
using dk.gi;
using dk.gi.cpr.servicelink;

//namespace dk.gi.app.console.template
namespace dk.gi.crm.app.anonymisering
{
    /// <summary>
    /// Din kode skal tilføjes i Metoden Start her i dette objekt
    /// </summary>
    public partial class GIConsoleApp
    {
        AppStatus appStatus = new AppStatus();

        /// <summary>
        /// Her udføres den egentlige behandling
        /// </summary>
        /// <returns>AppStatus</returns>
        private void CallBackFunction()
        {
            Trace.LogInformation("CallBackFunction blev kaldt");

            // ****************************************
            // Kør anonymisering af REGISTRERING 
            // ****************************************
            if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings["REGISTRERING"]))
            {
                CrmContext _crmcontext = new dk.gi.crm.CrmContext(appConfig.GetCrmConnectionString);
                Registrering.Anonymisering(_crmcontext, ref appStatus);
                _crmcontext = null;
            }

            // ****************************************
            // Kør anonymisering af VEDLIGEHOLD
            // ****************************************
            if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings["VEDLIGEHOLD"]))
            {
                CrmContext _crmcontext = new dk.gi.crm.CrmContext(appConfig.GetCrmConnectionString);
                if (System.Configuration.ConfigurationManager.AppSettings["afleveretRigsarkivet"] != "")
                {
                    string afleveretRigsarkivet = System.Configuration.ConfigurationManager.AppSettings["afleveretRigsarkivet"];
                    Vedligehold.Anonymisering(_crmcontext, afleveretRigsarkivet, ref appStatus);
                }
                else
                {
                    Vedligehold.Anonymisering(_crmcontext, null, ref appStatus);
                }
                _crmcontext = null;
            }

            // ****************************************
            // Kør anonymisering af TREKLIP
            // ****************************************
            if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings["TREKLIP"]))
            {
                CrmContext _crmcontext = new dk.gi.crm.CrmContext(appConfig.GetCrmConnectionString);
                TreKlip.Anonymisering(_crmcontext, ref appStatus);
                _crmcontext = null;
            }

            // ****************************************
            // Kør anonymisering af TEKNIKERSERVICE
            // Parameter ExplicitChangedOndate er ikke krævet men kan angives
            // ****************************************
            if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings["TEKNIKERSERVICE"]))
            {
                CrmContext _crmcontext = new dk.gi.crm.CrmContext(appConfig.GetCrmConnectionString);
                DateTime? ExplicitChangedOndate = null;  // En ikke krævet parameter
                //if (appConfig.ContainsKey("") == true)
                //    ExplicitChangedOndate = DateTime.Parse(appConfig["ExplicitChangedOndate"]);
                TeknikerService.Anonymisering(_crmcontext, ref appStatus, ExplicitChangedOndate);
                _crmcontext = null;
            }

            //ANJ*******

            // ****************************************
            // Kør anonymisering af LÅNUDENEJENDOM
            // ****************************************
            if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings["LAANUDENEJENDOM"]))
            {
                if (System.Configuration.ConfigurationManager.AppSettings["MODETYPE"] != "")
                {
                    CrmContext _crmcontext = new dk.gi.crm.CrmContext(appConfig.GetCrmConnectionString);
                    LaanUdenEjendom.Anonymisering(_crmcontext, ref appStatus, appConfig["MODETYPE"]);
                    _crmcontext = null;
                }
            }

            // ****************************************
            // Kør anonymisering af EJENDOM
            // ****************************************
            if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings["EJENDOM"]))
            {
                //ANJ FIX
                CrmContext _crmcontext = new dk.gi.crm.CrmContext(appConfig.GetCrmConnectionString);
                Ejendom.Anonymisering(_crmcontext, ref appStatus);
                _crmcontext = null;
            }

            // ****************************************
            // Kør anonymisering af KONTAKT
            // ****************************************
            if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings["KONTAKT"]))
            {
                //ANJ FIX
                CrmContext _crmcontext = new dk.gi.crm.CrmContext(appConfig.GetCrmConnectionString);
                Kontakt.Anonymisering(_crmcontext, ref appStatus);
                _crmcontext = null;
            }

            // ****************************************
            // Kør anonymisering af KONTAKTERMEDRELATIONTILULTIMATIVEJERTILANONYMISERING
            // ****************************************
            //if (mode.ToUpper() == "KONTAKTERMEDRELATIONTILULTIMATIVEJERTILANONYMISERING")
            //{
            //    //ANJ FIX
            //    app = KontakterMedRelationTilUltimativEjerTilAnonymisering.Anonymisering(crmcontext, ModeType);
            //}

            // ****************************************
            // Kør anonymisering af KONTAKTERMEDRELATIONTILKONTOTILANONYMISERING
            // ****************************************
            //if (mode.ToUpper() == "KONTAKTERMEDRELATIONTILKONTOTILANONYMISERING")
            //{
            //    //ANJ FIX
            //    app = KontakterMedRelationTilKontoEjerTilAnonymisering.Anonymisering(crmcontext, ModeType);
            //}

            Trace.LogInformation("CallBackFunction slut");
        }

        private void ModeXXX()
        {
            // Do something here and get a response
            /// ....
            // If response status is not OK return error text message
            //if (response.Status.IsErrorOrWarning())
            //    appStatus.SetStatusTekstmsg = response.Status.Message;
        }
    }
}