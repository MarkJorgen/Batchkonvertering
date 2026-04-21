using System;
using System.Collections.Generic;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Application.Abstractions
{
    public interface IRunoutRegistreringScanClient : IDisposable
    {
        ContactRegistreringUdloebneExecutionSummary VerifyConnection();
        IReadOnlyCollection<RunoutRegistreringCandidate> FindCandidates(Guid? registreringId);
        IReadOnlyCollection<Guid> GetClosedTreklipIds(IReadOnlyCollection<Guid> treklipIds);
        ResolvedServiceBusSettings ResolveServiceBusSettings();
    }
}
