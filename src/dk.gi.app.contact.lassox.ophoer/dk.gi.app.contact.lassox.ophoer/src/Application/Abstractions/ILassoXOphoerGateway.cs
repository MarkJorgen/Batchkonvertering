using dk.gi.app.contact.lassox.ophoer.Application.Models;

namespace dk.gi.app.contact.lassox.ophoer.Application.Abstractions
{
    public interface ILassoXOphoerGateway
    {
        LassoXOphoerExecutionSummary Execute(LassoXOphoerRequest request);
    }
}
