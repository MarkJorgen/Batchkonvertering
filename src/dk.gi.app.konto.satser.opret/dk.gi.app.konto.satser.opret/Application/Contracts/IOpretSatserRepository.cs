using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.satser.opret.Application.Models;

namespace dk.gi.app.konto.satser.opret.Application.Contracts
{
    public interface IOpretSatserRepository
    {
        Task<int> GetCandidateCountAsync(int satsAar, CancellationToken cancellationToken);
        Task<int> CreateAsync(int satsAar, CancellationToken cancellationToken);
    }
}
