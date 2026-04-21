using dk.gi.app.konto.satser.opret.Application;
using dk.gi.crm;
using Microsoft.Extensions.Logging;
using System;

namespace dk.gi.app.konto.satser.opret
{
    public partial class GIConsoleApp
    {
        private AppStatus.StateCode CallBackFunction()
        {
            Trace.LogInformation("CallBackFunction blev kaldt");

            int satsAar = DateTime.Today.Year + 1;
            var workflow = new OpretSatserWorkflow();
            OpretSatserWorkflowResult workflowResult = workflow.Execute(crmcontext, Trace, satsAar);

            if (workflowResult.StatusCode != AppStatus.StateCode.OK)
            {
                emailMessage = workflowResult.ErrorMessage;
            }

            Trace.LogInformation(
                $"CallBackFunction slut {workflowResult.StatusCode}. SatsAar={workflowResult.SatsAar}, Kandidater={workflowResult.CandidateCount}, Oprettet={workflowResult.CreatedCount}");

            return workflowResult.StatusCode;
        }
    }
}
