namespace FlowForge.Core.Entities;

public class StepLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StepExecutionId { get; set; }
    public StepExecution? StepExecution { get; set; }

    public string LogLevel { get; set; } = "INFO";
    public string Message { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
