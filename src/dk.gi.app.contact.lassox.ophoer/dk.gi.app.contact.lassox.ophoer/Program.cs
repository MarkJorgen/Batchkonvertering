using System;
using System.Globalization;
using System.Threading;
using Gi.Batch.Shared.Execution;
using dk.gi.app.contact.lassox.ophoer.Infrastructure.Composition;

namespace dk.gi.app.contact.lassox.ophoer
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("da-DK");

            try
            {
                var services = ServiceRegistry.Build(args);
                JobExecutionResult result = services.Orchestrator.Run();
                return result.ExitCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Jobbet stoppede før execution: " + ex.Message + " Se startup-diagnostik ovenfor.");
                Console.Error.WriteLine(ex);
                return 500;
            }
        }
    }
}
