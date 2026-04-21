using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.satser.slet.Application.Models;

namespace dk.gi.app.konto.satser.slet.Application.Contracts
{
    public interface ISatserRepository
    {
        Task<IReadOnlyCollection<SatserRecord>> GetCandidatesAsync(int satsAar, CancellationToken cancellationToken);

        Task DeleteAsync(SatserRecord candidate, CancellationToken cancellationToken);
    }
}
