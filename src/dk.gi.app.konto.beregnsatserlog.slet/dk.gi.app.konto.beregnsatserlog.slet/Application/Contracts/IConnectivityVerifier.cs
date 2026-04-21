using System.Threading;
using System.Threading.Tasks;

namespace dk.gi.app.konto.beregnsatserlog.slet.Application.Contracts
{
    public interface IConnectivityVerifier
    {
        Task VerifyAsync(CancellationToken cancellationToken);
    }
}
