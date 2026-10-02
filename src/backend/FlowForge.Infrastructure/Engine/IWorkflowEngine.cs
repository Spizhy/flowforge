using FlowForge.Core.Entities;

namespace FlowForge.Infrastructure.Engine;

public interface IWorkflowEngine
{
    Task<WorkflowRun> QueueRunAsync(Guid workflowDefinitionId, string triggeredBy, string inputPayloadJson, CancellationToken ct = default);
    Task ExecuteNextPendingRunAsync(
        Func<Guid, string, string, Task> onStepStatusChanged,
        Func<Guid, string, string, Task> onLogEmitted,
        CancellationToken ct = default);
}
