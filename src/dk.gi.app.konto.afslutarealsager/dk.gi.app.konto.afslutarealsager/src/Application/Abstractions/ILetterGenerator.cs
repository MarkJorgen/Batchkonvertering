using dk.gi.app.konto.afslutarealsager.Application.Models;

namespace dk.gi.app.konto.afslutarealsager.Application.Abstractions
{
    public interface ILetterGenerator
    {
        byte[] GeneratePdf(KontoAfslutArealSagerLetterMergeData mergeData);
    }
}
