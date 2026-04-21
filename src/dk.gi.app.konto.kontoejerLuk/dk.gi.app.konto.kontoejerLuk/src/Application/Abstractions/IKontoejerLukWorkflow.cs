using dk.gi.app.konto.kontoejerLuk.Application.Models;

namespace dk.gi.app.konto.kontoejerLuk.Application.Abstractions
{
    public interface IKontoejerLukWorkflow
    {
        KontoejerLukExecutionSummary Execute(KontoejerLukRequest request);
    }
}
