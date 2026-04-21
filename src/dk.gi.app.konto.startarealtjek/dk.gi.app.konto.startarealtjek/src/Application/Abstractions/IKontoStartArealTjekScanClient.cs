using System;
using System.Collections.Generic;
using dk.gi.app.konto.startarealtjek.Application.Models;

namespace dk.gi.app.konto.startarealtjek.Application.Abstractions
{
    public interface IKontoStartArealTjekScanClient : IDisposable
    {
        KontoStartArealTjekExecutionSummary VerifyConnection();
        KontoStartArealTjekBatchSettings ResolveBatchSettings();
        ResolvedServiceBusSettings ResolveServiceBusSettings();
        IReadOnlyCollection<KontoStartArealTjekAssessment> AssessAccounts(KontoStartArealTjekRequest request, KontoStartArealTjekBatchSettings batchSettings);
        void ApplySubjectFlagUpdates(IReadOnlyCollection<KontoStartArealTjekAssessment> assessments);
    }
}
