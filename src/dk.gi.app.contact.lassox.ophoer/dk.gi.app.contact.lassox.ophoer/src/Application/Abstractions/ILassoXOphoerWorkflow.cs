using dk.gi.app.contact.lassox.ophoer.Application.Models;

namespace dk.gi.app.contact.lassox.ophoer.Application.Abstractions
{
    public interface ILassoXOphoerWorkflow
    {
        LassoXOphoerExecutionSummary Execute(LassoXOphoerRequest request);
    }
}
