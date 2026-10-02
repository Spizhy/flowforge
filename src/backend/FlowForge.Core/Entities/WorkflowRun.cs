using FlowForge.Core.Enums;

namespace FlowForge.Core.Entities;

public class WorkflowRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowDefinitionId { get; set; }
    public WorkflowDefinition? WorkflowDefinition { get; set; }

    public RunStatus Status { get; set; } = RunStatus.Queued;
    public string TriggeredBy { get; set; } = "system";
    public string InputPayloadJson { get; set; } = "{}";
    public DateTime QueuedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? ErrorMessage { get; set; }

    public ICollection<StepExecution> StepExecutions { get; set; } = new List<StepExecution>();
}
