namespace FlowForge.Core.Entities;

public class WorkflowDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "Data Engineering";
    public string StepsJson { get; set; } = "[]"; // Serialized DAG definition
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<WorkflowRun> Runs { get; set; } = new List<WorkflowRun>();
}
