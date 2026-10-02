using Microsoft.EntityFrameworkCore;
using FlowForge.Core.Entities;
using FlowForge.Core.Enums;

namespace FlowForge.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(FlowForgeDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.WorkflowDefinitions.AnyAsync())
        {
            return;
        }

        var etlWorkflow = new WorkflowDefinition
        {
            Id = Guid.Parse("aaaaaaaa-1111-2222-3333-aaaaaaaaaaaa"),
            Name = "Enterprise Customer Ingestion & Vector Pipeline",
            Description = "Streams raw customer events from S3, anonymizes PII data, executes LLM embedding indexing, and dispatches webhook alerts.",
            Category = "Data Engineering",
            StepsJson = @"[
                {""stepId"": ""fetch-s3-payload"", ""name"": ""Ingest S3 Bucket Batch"", ""type"": ""Http"", ""dependsOn"": []},
                {""stepId"": ""validate-schema"", ""name"": ""Validate Schema & Deduplicate"", ""type"": ""Transform"", ""dependsOn"": [""fetch-s3-payload""]},
                {""stepId"": ""anonymize-pii"", ""name"": ""Anonymize PII & Redact Secrets"", ""type"": ""Transform"", ""dependsOn"": [""validate-schema""]},
                {""stepId"": ""index-vector-store"", ""name"": ""Generate Embeddings & Index"", ""type"": ""Script"", ""dependsOn"": [""anonymize-pii""]},
                {""stepId"": ""notify-completion"", ""name"": ""Broadcast Webhook Notification"", ""type"": ""Http"", ""dependsOn"": [""index-vector-store""]}
            ]",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
        };

        var modelRetrainWorkflow = new WorkflowDefinition
        {
            Id = Guid.Parse("bbbbbbbb-1111-2222-3333-bbbbbbbbbbbb"),
            Name = "Automated ML Drift Detection & Retrain DAG",
            Description = "Monitors daily prediction distribution, calculates Kolmogorov-Smirnov drift statistics, and conditionally triggers model weights re-sync.",
            Category = "Machine Learning",
            StepsJson = @"[
                {""stepId"": ""fetch-inference-logs"", ""name"": ""Pull Daily Inference Logs"", ""type"": ""Http"", ""dependsOn"": []},
                {""stepId"": ""calc-feature-drift"", ""name"": ""Evaluate Distribution Drift"", ""type"": ""Transform"", ""dependsOn"": [""fetch-inference-logs""]},
                {""stepId"": ""retrain-checkpoint"", ""name"": ""Trigger Fine-Tuning Job"", ""type"": ""Script"", ""dependsOn"": [""calc-feature-drift""]}
            ]",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
        };

        await context.WorkflowDefinitions.AddRangeAsync(etlWorkflow, modelRetrainWorkflow);

        // Seed initial completed execution run for telemetry
        var initialRun = new WorkflowRun
        {
            Id = Guid.Parse("cccccccc-1111-2222-3333-cccccccccccc"),
            WorkflowDefinitionId = etlWorkflow.Id,
            Status = RunStatus.Succeeded,
            TriggeredBy = "CronScheduler",
            QueuedAtUtc = DateTime.UtcNow.AddHours(-1),
            StartedAtUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(2),
            CompletedAtUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(45),
            StepExecutions = new List<StepExecution>
            {
                new()
                {
                    StepId = "fetch-s3-payload",
                    Name = "Ingest S3 Bucket Batch",
                    Type = "Http",
                    Status = StepStatus.Succeeded,
                    StartedAtUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(2),
                    CompletedAtUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(8),
                    Logs = new List<StepLog>
                    {
                        new() { LogLevel = "INFO", Message = "Connecting to s3://prod-ingest-bucket/2026/10/batch-992.parquet", TimestampUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(3) },
                        new() { LogLevel = "INFO", Message = "Downloaded 45,210 customer telemetry events (12.4 MB)", TimestampUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(7) }
                    }
                },
                new()
                {
                    StepId = "validate-schema",
                    Name = "Validate Schema & Deduplicate",
                    Type = "Transform",
                    Status = StepStatus.Succeeded,
                    StartedAtUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(8),
                    CompletedAtUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(15),
                    Logs = new List<StepLog>
                    {
                        new() { LogLevel = "INFO", Message = "Validating event records against JSON Schema v4", TimestampUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(9) },
                        new() { LogLevel = "INFO", Message = "Deduplication complete. Removed 142 duplicates.", TimestampUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(14) }
                    }
                },
                new()
                {
                    StepId = "anonymize-pii",
                    Name = "Anonymize PII & Redact Secrets",
                    Type = "Transform",
                    Status = StepStatus.Succeeded,
                    StartedAtUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(15),
                    CompletedAtUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(28)
                },
                new()
                {
                    StepId = "index-vector-store",
                    Name = "Generate Embeddings & Index",
                    Type = "Script",
                    Status = StepStatus.Succeeded,
                    StartedAtUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(28),
                    CompletedAtUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(42)
                },
                new()
                {
                    StepId = "notify-completion",
                    Name = "Broadcast Webhook Notification",
                    Type = "Http",
                    Status = StepStatus.Succeeded,
                    StartedAtUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(42),
                    CompletedAtUtc = DateTime.UtcNow.AddHours(-1).AddSeconds(45)
                }
            }
        };

        await context.WorkflowRuns.AddAsync(initialRun);
        await context.SaveChangesAsync();
    }
}
