/// <summary>
/// Koden her er den del af klassen GIConsoleApp (Partial)
///
/// Version: 2022 11 29
/// Sidste ændring: Rettet logning ved opstart
///
/// Når du opretter en ny applikation er det tanken at denne Program_App_ctor_template.cs kopieres til din app Program_App_ctor.cs (Og sættes til Compile)
/// Når du opgraderer NuGet Pakken "dk.gi.app.console.template.GiNugetSrc", så gentages kopieringen herover, men nu med overskriv (Har du rettet, så red dine rettelser først)
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
// Logging MEL Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
// Serilog
using Serilog;

// 
// Tilføj Nuget pakke dk.gi.library eller dk.gi.GINugetSrc - for at få adgang til trace som bruges i using herunder samt GISerilogTrace
// 
using dk.gi;
using dk.gi.app;

//namespace dk.gi.app.console.template
namespace dk.gi.crm.app.konto.afstemfinansposter
{

    /// <summary>
    /// Din kode skal tilføjes i Metoden Start her i dette objekt
    /// </summary>
    public partial class GIConsoleApp
    {
        #region Default .ctor
        /// <summary>
        /// Default .ctor for GIConsoleApp
        /// </summary>
        /// <param name="configure"></param>
        internal GIConsoleApp(GIAppConfigure configure)
        {
            // Diverse settings indlæs som det første i program
            appConfig = configure;

            // Flyt oprettelse af log til GIAPPConfigure og initier umiddelbart efter indlæsning af parametre
            //Program.WriteLineTempTraceLog($"Sporing dannes til filen:{configure.sporingsLog}");
            try
            {
                Program.WriteLineTempTraceLog("Opret Serilog");
                // Serilog oprettes som en Global defineret logger 
                Serilog.Log.Logger = new Serilog.LoggerConfiguration()
                .MinimumLevel.Verbose()  // Ellers kan vi ikke få Debug og Verbose med
                .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                //.WriteTo.File(configure.sporingsLog, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzzzz} {Level:u3}} {SourceContext} {Message:lj}{NewLine}{Exception}",
                //    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose)
                .CreateLogger();

                Program.WriteLineTempTraceLog("Opret LoggerFactory");
                // Opret et LoggerFactory obejtk
                var factory = new LoggerFactory();
                // Tilføj Serilog i "Fabrik"
                Program.WriteLineTempTraceLog("Add serilog to factory");
                factory.AddSerilog(Serilog.Log.Logger);

                // I stedet for og oprette en Microsoft.Extensions.Logging.ILogger 
                Program.WriteLineTempTraceLog("Tilføj factory til Logger");
                GILoggerProvider.SetLogFactory(factory);

                Program.WriteLineTempTraceLog("Set logger");
                Trace = dk.gi.GILoggerProvider.GetLogger(GetType().FullName);

                // I stedet for og oprette en Microsoft.Extensions.Logging.ILogger 
                Program.WriteLineTempTraceLog("Skriv til ny logger");
                Serilog.Log.Debug(System.DateTime.Now.ToLongTimeString() + " Serilog dannet");
                
                // Skriv program parametre til Trace
                foreach (var item in configure.appConfigureTable)
                {
                    string msg = $"Param key:{item.Key}, value:{item.Value}";
                    this.Trace.LogInformation(msg);
                    Program.WriteLineTempTraceLog(msg);
                }
                //
                //this.Trace = configure.Trace;

            }
            catch (Exception ex)
            {
                Program.WriteLineTempTraceLog($"Fejl:{ex.ToString()}");
            }
        }
        #endregion

        #region Lokale variable som bliver sat i initiering/config af app

        /// <summary>
        /// Indlæste environment variable, app args og appsettings
        /// </summary>
        internal GIAppConfigure appConfig { get; set; }

        /// <summary>
        /// En privat logger som default er sat til en NullLogger, den rettes/sættes så i Konstructor
        /// </summary>
        protected Microsoft.Extensions.Logging.ILogger Trace { get; private set; } = NullLogger.Instance;

        #endregion
    }
}