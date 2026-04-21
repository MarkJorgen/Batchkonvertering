/// <summary>
/// Koden her er den del af GI Skabelon til GIConsoleApp som er en hjælpeklasse for at gøre det lettere og hurtigere at opdatere skabelon kildekoden
/// 
/// Version: 2022 26 10 
/// Sidste ændring: Tilføjet variabel reuseserviceclient i appsettings
/// 
/// Når du opretter en ny applikation er det tanken at denne Program-template.cs kopieres til din app Program.cs (Og sættes til Compile)
/// Når du opgraderer NuGet Pakken "dk.gi.app.console.template.GiNugetSrc", så gentages kopieringen herover(Overskriver den eksisterende - Har du rettet, så red dine rettelser først) 
/// - Efterfølgende lægger du din kode ind i GIConsoleApp Start metoden. 
/// </summary>
// I Template husk at ret til rootnamespace med dollartegn foran og bagved

//namespace dk.gi.app.console.template
namespace dk.gi.crm.app.konto.lavsats
{
    // GI
    using dk.gi;
    using dk.gi.app;
    //
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    #region Eksempel AppSettings
    // <appSettings>
    //	 <!-- Microsoft Dynamics CRM -->
    //	 <add key="CrmConnectionTemplate" value="Url=https://{0}/{1}/;AuthType=AD;Username={2};Password={3};Domain=gisb" />
    //	 <add key="CrmServerName" value="intcrmtst.gi.dk" />
    //	 <add key="CrmOrganisationName" value="CRMTST" />
    //	 <add key="CrmUserName" value="crmtst@gisb.dk" />
    //	 <add key="CrmUserPassword" value="PgBaAEAAXwBwACMALgA7AFwAWABpAHcAawBGAFUAKAApAFEARAAlAA==" />
    //   <add key="TimeOutMinutter" value="2" />
    //   <add key="reuseserviceclient" value="true" />
    //	 <!-- Sporingsfiler lægges her -->
    //	 <add key="LogPath" value="C:\Temp\dk.gi.crm.app.konto.lavsats.logs" />
    //   <!--  Mail ved fejl -->
    //   <add key = "Azure.Email.ClientID" value="5b579852-6706-4ad4-be94-4510ffa99c52"/>
    //   <add key = "Azure.Email.ClientSecret" value="JwB8ACcAJgBAAG8AewBSAHQAZAAkAGsAYQBbACIARwBZADwAYwByAGsAdgBbACMAZwBWAGUAeABVAGkAewB9AFcAXgBkACIAYgA="/>
    //   <add key = "Azure.Email.TenantID" value="d5356f0d-2d9d-4c6c-86ed-f15d0c7f72e7"/>
    //   <add key = "Azure.Email.AfsenderEmail" value="no_reply@gi.dk" />
    //   <add key = "Azure.Email.Url" value="https://graph.microsoft.com/v1.0/users/{0}/sendMail"/>
    //   <add key = "modtagereEmail" value="jmw@gi.dk;hrl@gi.dk;rocnetahead@hotmail.com" />
    //	 <!-- Microsoft Message Queue -->
    //	 <add key="gimsmqServer" value="GICRM90TST"/>
    //		<!-- Styring af synkronisering, flere instanser skal ikke køre samtidigt -->
    //		<add key="SecondsToSleep" value="25" />
    //		<add key="MaxWaitCount" value="5" />
    // </appSettings>
    #endregion
    // Tilføj Nuget pakke dk.gi.library eller dk.gi.GINugetSrc - for at få adgang til trace

    /// <summary>
    /// Her startet programmet
    /// Dette er en "partial" del af Program, 
    /// - her befinder sig den del af koden der som oftest ikke skal ændres.
    /// </summary>
    public partial class Program
    {
        // Find navn på den aktuelle applikation, skal bruges til sporing og evt. fejl mails
        public static string appName = Assembly.GetExecutingAssembly().ManifestModule.Name;
        public static string appStart = System.DateTime.Now.ToString("yyyyMMdd-HHmmssff");

        /// <summary>
        /// Microsoft officielle Main - Default program start
        /// In general, the console reads input and writes output by using the current console code page, which the system locale defines by default. 
        /// A code page can handle only a subset of available Unicode characters, so if you try to display characters that are not mapped by a particular code page, 
        /// - the console won't be able to display all characters or represent them accurately.
        /// </summary>
        /// <param name="args">parametre som indlæses fra kommando linjen</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        static int Main(string[] args)
        {
            // Convert args to a list to add a new element
            List<string> argsList = args.ToList();
            argsList.Add("-MODE=BATCH");
            args = argsList.ToArray();

            // Som det første sætter vi aktuel program tråd til dansk kultur, så vi får tal/dato rigtig
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("da-DK");
            /// 20220310 JMW rettet fra integer til AppStatus
            AppStatus appstatus = new AppStatus(); // Default er alt ok.
            // Vi har indtil videre ingen trace, så det eneste vi kan gøre - er at skrive til Console
            WriteLineTempTraceLog($"[{appName}]Program.Main start:{System.DateTime.Now.ToLongTimeString()}");

            WriteLineTempTraceLog($"[{appName}]Program.Main Før WaitIfApplicationActive");
            //************************************************************************************************************************************
            // Her valideres om app allerede kører i forvejen, og dermed kan vi vente lidt i de tilfælde  vi ikke vil have flere instanser kørende samtidigt
            // Åben for koden i funktionen WaitIfApplicationActive, hvis du har brug for denne funktionalitet, du kan ikke samtidigt have IsApplicationActive koden åbent
            // Parametre: Første antal sekunder "sleep" og andet er Max antal gange der "sleep's"
            //************************************************************************************************************************************
            //int antal = WaitIfApplicationActive(5, 12);
            // Her kan du så gøre et eller andet hvis antal ikke er 0

            WriteLineTempTraceLog($"[{appName}]Program.Main Før IsApplicationActive");
            //************************************************************************************************************************************
            // Her valideres om app allerede kører i forvejen, og dermed kan vi stoppe den hvis vi ikke vil have flere instanser kørende samtidigt
            // Åben for koden i funktionen IsApplicationActive, hvis du har brug for denne funktionalitet, du kan ikke samtidt have WaitIfApplicationActive koden åben
            //************************************************************************************************************************************
            //if (IsApplicationActive() == true)
            //    appstatus.SetStatus = AppStatus.StateCode.AppIsRunning;  // 20220310 JMW Tilføjet denne

            // Her begynder den egentlige behandling
            WriteLineTempTraceLog($"[{appName}]Program.Main start egentlig behandling pakket ind i try/catch");
            // 20220310 JMW Tilføjet test på at der ikke er opstået fejl undervejs
            if (appstatus.statecode == AppStatus.StateCode.OK)
            {
                try
                {
                    WriteLineTempTraceLog($"[{appName}]Program.Main Opret GIAppConfigure");
                    //******************************************************************************
                    // Indlæs configuration fra OS Environmen, parametre(args) og fra app.config fil
                    // husk at tilføje de navne i ekstraparametre som du skal bruge i dette program.
                    //******************************************************************************
                    GIAppConfigure configure = new GIAppConfigure(appName, args, ekstraParametre);

                    WriteLineTempTraceLog($"[{appName}]Program.Main Kald SetTraceMode");
                    //******************************************************************************
                    // Definer om der dannes et ekstra directory for Mode parameter
                    //******************************************************************************
                    //SetTraceMode(ref configure);

                    WriteLineTempTraceLog($"[{appName}]Program.Main Kald SetKraevedeParametre");
                    //******************************************************************************
                    // Definer parametre som skal være til stede i programmet
                    //******************************************************************************
                    SetKraevedeParametre(ref configure);

                    WriteLineTempTraceLog($"[{appName}]Program.Main Kald ValidateKraevedeParametre");
                    //******************************************************************************
                    // Validering af parametre
                    //******************************************************************************
                    appstatus.SetStatus = ValidateKraevedeParametre(ref configure);

                    // Hvis validering var forskelligt fra nul, så udfør ikke mere her
                    if (appstatus.statecode == AppStatus.StateCode.OK)
                    {
                        WriteLineTempTraceLog($"[{appName}]Program.Main opret GIConsoleApp");
                        //******************************************************************************
                        // Så er vi klar til at køre vores program
                        // Opret et objekt hvor vi kan lægge programs kode, uden at vi skal lave rettelser her i Program.cs,
                        // og dermed gøre fremtidige rettelser lettere
                        GIConsoleApp app = new GIConsoleApp(configure);
                        // Start app
                        WriteLineTempTraceLog($"[{appName}]Program.Main kald GIConsoleApp.Start");
                        appstatus = app.Start();    // læg den ønskede kode der skal udføres ind i Start metoden  ***
                        WriteLineTempTraceLog($"[{appName}]Program.Main kald GIConsoleApp.Start slut:{appstatus.GetStatusTekstmsg}");
                    }
                    ///******************************************************************************
                    /// Færdig
                }
                catch (Exception ex)
                {
                    // Her fanges de generelle fejl som opstår i den generelle del af Program.cs og GIConsoleApp, Bemærk at din kode i GIConsoleApp.Start er pakket ind i Try catch 
                    WriteLineTempTraceLog($"[{appName}] Program.Main Exception{ex.ToString()}");
                }
            }

            WriteLineTempTraceLog($"[{appName}] Program.Main slut:{System.DateTime.Now.ToLongTimeString()}");
            //System.Threading.Thread.Sleep(3000); // 3 sekunders pause
            //WriteLineTempTraceLog("Tryk på en tast!")
            //Console.ReadKey();

            // Hvis alt går godt, så slet den midlertidige trace log fil
            if (appstatus.statecode == AppStatus.StateCode.OK || appstatus.statecode == AppStatus.StateCode.AppIsRunning)
            {
                //DeleteTemTraceLog();
            }
            else
                    WriteLineTempTraceLog($"[{appName}] Program.Main slut: Program sluttede med fejl:{appstatus.GetStatusTekstmsg}, denne fil skal slettes manuelt!");

            // Afslut program og retuner integer til OS
            return (int)appstatus.statecode;
        }

        #region Midlertidig trace fil
        /// <summary>
        /// Formål med denne er og give en midlertidig trace fil af hvad der sker i programmet indtil den "rigtige" tracelog er startet
        /// </summary>
        /// <param name="trace"></param>
        private static void WriteLineTempTraceLog(string trace)
        {
            try
            {
                // Først skrives trace til console
                Console.WriteLine(trace);
                // Dernæst til fil
                //File.AppendAllText(GetTraceFilename(), trace + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Slet filen efter brug
        /// </summary>
        //private static void DeleteTemTraceLog()
        //{
        //    try
        //    {
        //        if (File.Exists(GetTraceFilename()) == true)
        //            File.Delete(GetTraceFilename());
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        /// <summary>
        /// Nanv på midlertidig trace fil
        /// Default ligger den i C:\Temp som findes på alle vores servere
        /// </summary>
        /// <returns></returns>
        //private static string GetTraceFilename()
        //{
        //    return "C:\\Temp\\" + appName + appStart + ".trace";
        //}
        #endregion
    }
}