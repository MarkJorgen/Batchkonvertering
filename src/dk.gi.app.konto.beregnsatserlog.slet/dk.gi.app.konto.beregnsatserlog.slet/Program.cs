using System;
using System.Globalization;
using System.Threading;
using dk.gi.app.konto.beregnsatserlog.slet.Application.Services;
using dk.gi.app.konto.beregnsatserlog.slet.Infrastructure.Composition;
using dk.gi.app.konto.beregnsatserlog.slet.Infrastructure.Support;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace dk.gi.app.konto.beregnsatserlog.slet
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("da-DK");

            ILoggerFactory loggerFactory = NullLoggerFactory.Instance;
            ILogger logger = loggerFactory.CreateLogger("Program");

            try
            {
                var mergedSettings = FlatJsonSettingsLoader.LoadMerged(args);
                StartupDiagnosticsWriter.Write(logger, "dk.gi.app.konto.beregnsatserlog.slet", mergedSettings);

                var settings = SletBeregnSatserLogSettingsFactory.Create(mergedSettings, args);
                var validator = new SletBeregnSatserLogSettingsValidator();
                validator.ValidateAndThrow(settings);

                var registry = new ServiceRegistry(loggerFactory, settings);
                var orchestrator = registry.CreateOrchestrator();
                var report = orchestrator.ExecuteAsync(settings).GetAwaiter().GetResult();

                Console.WriteLine(
                    "[INFO] Job færdigt. Mode={0}, Candidates={1}, Deleted={2}, ConnectivityVerified={3}",
                    settings.Mode,
                    report.CandidateCount,
                    report.DeletedCount,
                    report.ConnectivityVerified);

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Jobbet fejlede: " + ex.Message);
                Console.Error.WriteLine(ex);
                return 1;
            }
        }
    }
}
