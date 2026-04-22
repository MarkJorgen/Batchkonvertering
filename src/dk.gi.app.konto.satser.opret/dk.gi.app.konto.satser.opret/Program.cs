using System;
using System.Globalization;
using System.Threading;
using dk.gi.app.konto.satser.opret.Application.Services;
using dk.gi.app.konto.satser.opret.Infrastructure.Composition;
using dk.gi.app.konto.satser.opret.Infrastructure.Support;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace dk.gi.app.konto.satser.opret
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
                StartupDiagnosticsWriter.Write(logger, "dk.gi.app.konto.satser.opret", mergedSettings);

                var settings = OpretSatserSettingsFactory.Create(mergedSettings, args);
                var validator = new OpretSatserSettingsValidator();
                validator.ValidateAndThrow(settings);

                var registry = new ServiceRegistry(loggerFactory, settings);
                var orchestrator = registry.CreateOrchestrator();
                var report = orchestrator.ExecuteAsync(settings).GetAwaiter().GetResult();

                Console.WriteLine(
                    "[INFO] Job færdigt. Mode={0}, SatsAar={1}, Candidates={2}, Created={3}, ConnectivityVerified={4}",
                    settings.Mode,
                    settings.SatsAar,
                    report.CandidateCount,
                    report.CreatedCount,
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
