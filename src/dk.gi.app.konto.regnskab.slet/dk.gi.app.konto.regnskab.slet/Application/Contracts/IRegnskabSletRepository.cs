using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.regnskab.slet.Application.Models;

namespace dk.gi.app.konto.regnskab.slet.Application.Contracts
{
    public interface IRegnskabSletRepository
    {
        Task<IReadOnlyCollection<KontoCandidate>> GetCandidatesAsync(CancellationToken cancellationToken);
        Task<ResolvedServiceBusSettings> ResolveServiceBusSettingsAsync(CancellationToken cancellationToken);
    }
}
