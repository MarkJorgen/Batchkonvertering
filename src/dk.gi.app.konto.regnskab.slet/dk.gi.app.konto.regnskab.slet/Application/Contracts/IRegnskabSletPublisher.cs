using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.regnskab.slet.Application.Models;

namespace dk.gi.app.konto.regnskab.slet.Application.Contracts
{
    public interface IRegnskabSletPublisher
    {
        Task<int> PublishAsync(IReadOnlyCollection<KontoCandidate> candidates, ResolvedServiceBusSettings resolvedServiceBusSettings, CancellationToken cancellationToken);
    }
}
