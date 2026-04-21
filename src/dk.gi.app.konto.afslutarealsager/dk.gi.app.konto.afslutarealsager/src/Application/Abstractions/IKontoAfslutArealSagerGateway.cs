using dk.gi.app.konto.afslutarealsager.Application.Models;

namespace dk.gi.app.konto.afslutarealsager.Application.Abstractions
{
    public interface IKontoAfslutArealSagerGateway
    {
        KontoAfslutArealSagerExecutionSummary Execute(KontoAfslutArealSagerRequest request);
    }
}
