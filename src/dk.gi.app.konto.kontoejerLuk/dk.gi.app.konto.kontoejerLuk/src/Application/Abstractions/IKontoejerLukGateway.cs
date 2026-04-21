using dk.gi.app.konto.kontoejerLuk.Application.Models;

namespace dk.gi.app.konto.kontoejerLuk.Application.Abstractions
{
    public interface IKontoejerLukGateway
    {
        KontoejerLukExecutionSummary Execute(KontoejerLukRequest request);
    }
}
