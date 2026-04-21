using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.beregnsatserlog.slet.Application.Models;

namespace dk.gi.app.konto.beregnsatserlog.slet.Application.Contracts
{
    public interface IBeregnSatserLogRepository
    {
        Task<IReadOnlyCollection<BeregnSatserLogRecord>> GetCandidatesAsync(int olderThanYears, CancellationToken cancellationToken);

        Task DeleteAsync(BeregnSatserLogRecord candidate, CancellationToken cancellationToken);
    }
}
