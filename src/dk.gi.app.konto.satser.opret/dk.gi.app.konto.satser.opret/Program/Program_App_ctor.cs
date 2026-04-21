/// <summary>
/// Koden her er den del af klassen GIConsoleApp (Partial)
///
/// Version: 2022 10 05
/// Sidste ændring: Rettet fejl, forkert rækkefølge på log/sporing
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
namespace dk.gi.app.konto.satser.opret
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
			//Console.WriteLine($"Sporing dannes til filen:{configure.sporingsLog}");

			// Serilog oprettes som en Global defineret logger 
			Serilog.Log.Logger = new Serilog.LoggerConfiguration()
				.MinimumLevel.Verbose()  // Ellers kan vi ikke få Debug og Verbose med
				.WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                //.WriteTo.File(configure.sporingsLog, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzzzz} {Level:u3}} {SourceContext} {Message:lj}{NewLine}{Exception}",
                //    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose)
				.CreateLogger();

            // Opret et LoggerFactory obejtk
            var factory = new LoggerFactory();
            // Tilføj Serilog i "Fabrik"
            factory.AddSerilog(Serilog.Log.Logger);

            // I stedet for og oprette en Microsoft.Extensions.Logging.ILogger 
            GILoggerProvider.SetLogFactory(factory);
            Trace = dk.gi.GILoggerProvider.GetLogger(GetType().FullName);

            Serilog.Log.Debug(System.DateTime.Now.ToLongTimeString() + " Serilog dannet");
            Trace.LogInformation("Microsoft.Extensions.Logging.ILogger sendes til Serilog");

			// Skriv program parametre til Trace
			foreach (var item in configure.appConfigureTable)
			{
				this.Trace.LogInformation($"Param key:{item.Key}, value:{item.Value}");
			}
			//
			//this.Trace = configure.Trace;
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
		internal Microsoft.Extensions.Logging.ILogger Trace { get; private set; } = NullLogger.Instance;

		#endregion
	}
}