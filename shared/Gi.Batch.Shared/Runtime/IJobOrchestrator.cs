using Gi.Batch.Shared.Execution;

namespace Gi.Batch.Shared.Runtime
{
    public interface IJobOrchestrator
    {
        JobExecutionResult Run();
    }
}
