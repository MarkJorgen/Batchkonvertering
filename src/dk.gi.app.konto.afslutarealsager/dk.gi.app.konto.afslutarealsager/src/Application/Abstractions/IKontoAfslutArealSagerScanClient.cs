using System;
using System.Collections.Generic;
using dk.gi.app.konto.afslutarealsager.Application.Models;

namespace dk.gi.app.konto.afslutarealsager.Application.Abstractions
{
    public interface IKontoAfslutArealSagerScanClient : IDisposable
    {
        void EnsureConnection();
        IReadOnlyList<KontoAfslutArealSagerCandidate> GetOpenCases(KontoAfslutArealSagerRequest request);
        IReadOnlyList<KontoAfslutArealSagerCandidate> DiscoverCases(KontoAfslutArealSagerRequest request, int limit);
        Guid CreateLetterActivity(KontoAfslutArealSagerCandidate candidate, KontoAfslutArealSagerRequest request);
        void UploadLetterToActivity(Guid activityId, KontoAfslutArealSagerCandidate candidate, byte[] pdfBytes);
        void CompleteActivity(Guid activityId);
        void StageDigitalPostNote(Guid activityId, KontoAfslutArealSagerCandidate candidate, byte[] pdfBytes, string title);
        void CloseIncident(KontoAfslutArealSagerCandidate candidate, int statusCode, string resolutionSubject, string resolutionDescription);
        KontoAfslutArealSagerArealCarryForwardResult CarryForwardOpenArea(KontoAfslutArealSagerCandidate candidate, bool deleteZeroRegnskab);
        ResolvedServiceBusSettings ResolveServiceBusSettings();
    }
}
