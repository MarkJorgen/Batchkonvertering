// GI
using dk.gi;
using dk.gi.crm;
using dk.gi.crm.giproxy;
using dk.gi.crm.managers;
using dk.gi.crm.managers.V2;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.crm.app.konto.godkender.nbgl
{
    /// <summary>
    /// En opdatering af alle selskaber med Nej i KDK indhentet og hvor alle reelleejere har ja
    /// - efter nogen tid var der enkelte selskaber der stod til nej, selvom de burde være ja, (Alle underliggende ultimative ejere var ja)
    /// </summary>
    /// <remarks>
    /// Oprettet af JMW 2019 11 04
    /// </remarks>
    public class TilfoejCVROgGodkendereRequest : CrmRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public TilfoejCVROgGodkendereRequest(CrmContext context) : base(context) { }

        #region Angivelse af Egenskaber/Properties
        /*  Brug af denne funktionalitet Kræver "using System.Runtime.Serialization;"
         *  
         *   Det er kun properties med DataMember IsRequired = true, der auto-valideres, - og med properties menes kun dem med en get; og set;
         *   Kun "built in typer" kan valideres. String, int, Guid osv ikke CRM klasser som f.eks. Entity
         *  
         *  Request bliver automatisk valideret og skrevet til trace fra den Abstrakte klasse som der arves fra.
         *   Sættes "[GIRequestTraceAttribute(AddPropertyToTrace = false)]" på klassen skrives properties ikke til trace
         *
         *  Validering sker når der tilføjes [DataMember] til en property, 
         *   Kun properties med DataMember IsRequired = true, valideres, hvis ikke skrives indhold bare til trace
         *   [IgnoreDataMemberAttribute], så kommer den ikke med i validering/trace
         *   
         *   Bemærk: Der SKAL være "{ get; set; }" på felt, ellers er det IKKE en property og bliver så ikke valideret
         * 
         *   [DataMember]
         *   public int MitHeltal { get; set; }
         *
         *   [DataMember(IsRequired = true)]
         *   public string KontoNr { get; set; }
         *   
         *   [IgnoreDataMemberAttribute]
         *   public string Password { get; set; }
         *   
         */
        #endregion



        protected override IResponse ExecuteRequest()
        {
            // Opret vores result
            var result = new TilfoejCVROgGodkendereResponse();

            Guid godkender1 = Guid.Empty;
            Guid godkender2 = Guid.Empty;
            string GI_CVR = "26092515";

            /*
                1. hent alle konti
                2. Hent til de 2 variabler (godkender1,godkender2)
                3. gennemgå alle hentede konti, udfyld godkendere efter behov.
            */

            using (Ap_KontoManager managerkonto = new Ap_KontoManager(this.localCrmContext))
            using (SystemUserManager systemUserManager = new SystemUserManager(this.localCrmContext))
            using (ConfigurationSettingsManager konfigManager = new ConfigurationSettingsManager(this.localCrmContext))
            {
                //Hent alle aktive konti
                string[] attributter = new string[] {
                    AP_konto.Fields.Id, AP_konto.Fields.AP_Kontonr,  AP_konto.Fields.AP_Godkender1dato,
                    AP_konto.Fields.ap_godkender1id, AP_konto.Fields.AP_Godkender1dato,
                    AP_konto.Fields.ap_godkender2id, AP_konto.Fields.AP_Godkender2dato,
                    AP_konto.Fields.AP_Udbetalingsbankkonto, AP_konto.Fields.AP_UdbetalingsCPR,
                    AP_konto.Fields.AP_UdbetalingsCVR, AP_konto.Fields.AP_bindingspligt, AP_konto.Fields.ap_lovgrundlag
                };

                List<AP_konto> kontoliste = managerkonto.HentAlle(true, attributter).ToList<AP_konto>();
                if (kontoliste != null)
                {
                    this.Trace.LogInformation($"Fandt { kontoliste.Count} aktive konti i CRM");
                    kontoliste = kontoliste.OrderBy(x => x.AP_Godkender1dato).ToList(); //order list by date ascending
                    kontoliste.Reverse(); //Sætter nyeste godkendte først
                }
                else
                {
                    string msg = "TilfoejCVROgGodkendereRequest returnerede fejl i data forespørgsel af konti!";
                    throw new Exception(msg);
                }

                //Find godkenderID1 og godkenderID2 af den sidste godkendte konto, hvor de ikke er ens. Det er bare 2 tilfældige godkendere..
                //this.Trace.LogInformation($"Ser efter 2 godkendere som kan indsættes.");
                //foreach (AP_konto kontodata in kontoliste)
                //{
                //    bool hargodkender1 = false;
                //    bool hargodkender2 = false;

                //    Trace.LogInformation("konto: " + kontodata.AP_Kontonr);

                //    if (kontodata.Contains("ap_godkender1id") == true)
                //    {
                //        Trace.LogInformation("godkender1 fundet: " + kontodata.ap_godkender1id.Id);
                //        hargodkender1 = true;
                //    }
                //    else
                //    {
                //        Trace.LogInformation("godkender1 er ikke specificeret!");
                //    }

                //    if (kontodata.Contains("ap_godkender2id") == true)
                //    {
                //        Trace.LogInformation("godkender2 fundet: " + kontodata.ap_godkender2id.Id);
                //        hargodkender2 = true;
                //    }
                //    else
                //    {
                //        Trace.LogInformation("godkender2 er ikke specificeret!");
                //    }

                //    if (hargodkender1 == true && hargodkender2 == true) //find den første hvor begge godkender er sat samtidigt
                //    {
                //        if (kontodata.ap_godkender1id.Id != kontodata.ap_godkender2id.Id) //Det er ikke den samme godkender (bør ikke være muligt)
                //        {
                //            godkender1 = kontodata.ap_godkender1id.Id;
                //            godkender2 = kontodata.ap_godkender2id.Id;
                //            break; //Exit foreach
                //        }
                //        else
                //        {
                //            Trace.LogError("Konto nummer: " + kontodata.AP_Kontonr + " har den samme person som begge godkendere! Det må det nok ikke.");
                //            continue; //videre til den næste, godkender er vist den samme person.. det er underligt.
                //        }
                //    }
                //    else
                //    {
                //        continue; //hop over denne, den har ikke begge godkendere samtidigt..
                //    }
                //}

                this.Trace.LogInformation($"Henter godkender 1 og godkender 2.");

                godkender1 = systemUserManager.Hent(konfigManager.Hent("App.gi.konto.bruger")).ToEntityReference().Id;
                godkender2 = systemUserManager.Hent(konfigManager.Hent("App.gi.crmprocesser.bruger")).ToEntityReference().Id;

                Trace.LogInformation($"Godkender 1 = {godkender1} og godkender 2 = {godkender2}.");


                //Nu er begge godkendere fundet, gennemgå alle konti fra start igen.
                foreach (AP_konto kontodata in kontoliste)
                {
                    /*
                     1. Hvis bindingspligt er et positivt tal, så skal konto ikke behandles (region "Test på bindingspligt")
                     2. Hvis lovgrundlag er ændret, (Efterjuni2015), skal konto ikke behandles. (region "Test på lovgrundlag")
                     3. Udfyld godkender som mangler, hvis de har udbetalingsoplysninger, ellers skip it. (region "Udfyld godkendere som mangler")
                     4. Opdater CRM med nye konto informationer (godkender/cvr m.v.)
                     5. Videre i loop
                    */

                    string logLinje = $"Konto {kontodata.AP_Kontonr}. ";
                    bool updatekonto = false;
                    AP_konto nyKonto = new AP_konto { Id = kontodata.Id, };

                    #region Test på bindingspligt og exit foreach hvis større end 0
                    decimal bindingspligt = kontodata.AP_bindingspligt.GetValueOrDefault();

                    logLinje += $"Bindingspligt = {bindingspligt}. ";

                    if (bindingspligt >= 0)
                    {
                        logLinje += "Grundet positiv bindingspligt, ændres der ikke på kontoen.";
                        Trace.LogInformation(logLinje);
                        continue;
                    }
                    #endregion

                    #region Test på lovgrundlag og exit foreach loop hvis det er
                    bool erUnderNytLovgrundlag = kontodata.ap_lovgrundlag.GetValueOrDefault() == (int)AP_konto_ap_lovgrundlag.Efterjuni2015;
                    logLinje += $"Lovgrundlag = {((AP_konto_ap_lovgrundlag)kontodata.ap_lovgrundlag.GetValueOrDefault()).ToString()}.";
                    if (erUnderNytLovgrundlag)
                    {
                        logLinje += "Grundet lovgrundlaget, ændres der ikke på kontoen.";
                        Trace.LogInformation(logLinje);
                        continue;
                    }
                    #endregion

                    #region Udfyld godkendere som mangler
                    bool harGodkender1 = kontodata.ap_godkender1id != null;
                    bool harGodkender2 = kontodata.ap_godkender2id != null;
                    bool harUdbetalingsoplysning = !string.IsNullOrWhiteSpace(kontodata.AP_Udbetalingsbankkonto) || !string.IsNullOrWhiteSpace(kontodata.AP_UdbetalingsCPR) || !string.IsNullOrWhiteSpace(kontodata.AP_UdbetalingsCVR);



                    if (harUdbetalingsoplysning)
                    {
                        if (harGodkender1 == false || harGodkender2 == false)
                        {
                            logLinje += $"Har udbetalingsoplysninger, men mangler godkendere. Tilføjer godkendere og erstatter udbetalingsoplysninger. ";

                            logLinje += $"Sætter Godkender 1. {godkender1}";
                            nyKonto.ap_godkender1id = new EntityReference(SystemUser.EntityLogicalName, godkender1);
                            nyKonto.AP_Godkender1dato = DateTime.Now;

                            logLinje += $"Sætter Godkender 2. {godkender2}";
                            nyKonto.ap_godkender2id = new EntityReference(SystemUser.EntityLogicalName, godkender2);
                            nyKonto.AP_Godkender2dato = DateTime.Now;

                            logLinje += "Erstatter udbetalingsoplysninger. ";
                            nyKonto.AP_Udbetalingsbankkonto = null;
                            nyKonto.AP_UdbetalingsCPR = null;
                            nyKonto.AP_UdbetalingsCVR = GI_CVR;
                            updatekonto = true;
                        }
                        else
                            logLinje += $"Har én godkender og udbetalingsoplysninger, kontoen vil ikke blive ændret. ";
                    }
                    else
                    {
                        logLinje += $"Sætter CVR for udbetalingsoplysninger. ";
                        nyKonto.AP_UdbetalingsCVR = GI_CVR;

                        // Test på godkender informationer
                        if (kontodata.ap_godkender1id == null)
                        {
                            logLinje += $"Sætter Godkender 1. {godkender1}";
                            nyKonto.ap_godkender1id = new EntityReference(SystemUser.EntityLogicalName, godkender1);
                            nyKonto.AP_Godkender1dato = DateTime.Now;
                            updatekonto = true;
                        }

                        if (kontodata.ap_godkender2id == null)
                        {
                            logLinje += $"Sætter Godkender 2. {godkender2}";
                            nyKonto.ap_godkender2id = new EntityReference(SystemUser.EntityLogicalName, godkender2);
                            nyKonto.AP_Godkender2dato = DateTime.Now;
                            updatekonto = true;
                        }
                    }
                    #endregion

                    //Opdater konto med ændringer
                    if (updatekonto == true)
                    {
                        managerkonto.Update(nyKonto);
                        logLinje += $"\r\nKonto nummer: { kontodata.AP_Kontonr } er opdateret!";
                    }
                    Trace.LogInformation(logLinje);
                }
            }
            return result;

        }
    }
}


