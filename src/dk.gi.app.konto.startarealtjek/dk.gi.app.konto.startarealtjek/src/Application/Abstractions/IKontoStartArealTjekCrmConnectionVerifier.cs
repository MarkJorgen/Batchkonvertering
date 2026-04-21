using dk.gi.app.konto.startarealtjek.Application.Models;

namespace dk.gi.app.konto.startarealtjek.Application.Abstractions
{
    public interface IKontoStartArealTjekCrmConnectionVerifier
    {
        KontoStartArealTjekExecutionSummary Verify();
    }
}
