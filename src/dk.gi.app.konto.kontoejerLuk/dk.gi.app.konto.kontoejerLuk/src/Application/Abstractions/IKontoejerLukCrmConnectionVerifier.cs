using dk.gi.app.konto.kontoejerLuk.Application.Models;

namespace dk.gi.app.konto.kontoejerLuk.Application.Abstractions
{
    public interface IKontoejerLukCrmConnectionVerifier
    {
        KontoejerLukExecutionSummary Verify();
    }
}
