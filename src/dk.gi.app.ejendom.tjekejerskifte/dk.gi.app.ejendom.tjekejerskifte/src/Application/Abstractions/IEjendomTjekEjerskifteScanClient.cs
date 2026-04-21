using System;
using System.Collections.Generic;
using dk.gi.app.ejendom.tjekejerskifte.Application.Models;
namespace dk.gi.app.ejendom.tjekejerskifte.Application.Abstractions
{
    public interface IEjendomTjekEjerskifteScanClient : IDisposable
    {
        EjendomTjekEjerskifteExecutionSummary VerifyConnection();
        IReadOnlyCollection<EjendomTjekEjerskifteCandidate> GetCandidates(EjendomTjekEjerskifteRequest request);
        ResolvedServiceBusSettings ResolveServiceBusSettings();
    }
}
