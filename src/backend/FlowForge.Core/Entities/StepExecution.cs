using FlowForge.Core.Enums;

namespace FlowForge.Core.Entities;

public class StepExecution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowRunId { get; set; }
    public WorkflowRun? WorkflowRun { get; set; }

    public string StepId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Http"; // Http, Sql, Script, Transform, Email
    public StepStatus Status { get; set; } = StepStatus.Pending;

    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? OutputDataJson { get; set; }
    public string? ErrorMessage { get; set; }

    public ICollection<StepLog> Logs { get; set; } = new List<StepLog>();
}
