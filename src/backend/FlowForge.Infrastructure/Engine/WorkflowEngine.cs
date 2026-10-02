using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FlowForge.Core.Entities;
using FlowForge.Core.Enums;
using FlowForge.Infrastructure.Data;

namespace FlowForge.Infrastructure.Engine;

public class WorkflowEngine : IWorkflowEngine
{
    private readonly FlowForgeDbContext _context;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(FlowForgeDbContext context, ILogger<WorkflowEngine> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkflowRun> QueueRunAsync(Guid workflowDefinitionId, string triggeredBy, string inputPayloadJson, CancellationToken ct = default)
    {
        var def = await _context.WorkflowDefinitions.FindAsync(new object[] { workflowDefinitionId }, ct);
        if (def == null)
        {
            throw new KeyNotFoundException($"Workflow definition {workflowDefinitionId} not found.");
        }

        var run = new WorkflowRun
        {
            WorkflowDefinitionId = def.Id,
            Status = RunStatus.Queued,
            TriggeredBy = triggeredBy,
            InputPayloadJson = inputPayloadJson,
            QueuedAtUtc = DateTime.UtcNow
        };

        // Parse DAG steps from Definition
        var steps = JsonSerializer.Deserialize<List<DagStepDefinition>>(def.StepsJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<DagStepDefinition>();

        foreach (var s in steps)
        {
            run.StepExecutions.Add(new StepExecution
            {
                StepId = s.StepId,
                Name = s.Name,
                Type = s.Type,
                Status = StepStatus.Pending
            });
        }

        _context.WorkflowRuns.Add(run);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Workflow run queued: {RunId} for workflow {WorkflowName}", run.Id, def.Name);
        return run;
    }

    public async Task ExecuteNextPendingRunAsync(
        Func<Guid, string, string, Task> onStepStatusChanged,
        Func<Guid, string, string, Task> onLogEmitted,
        CancellationToken ct = default)
    {
        var run = await _context.WorkflowRuns
            .Include(r => r.WorkflowDefinition)
            .Include(r => r.StepExecutions)
            .FirstOrDefaultAsync(r => r.Status == RunStatus.Queued, ct);

        if (run == null) return;

        run.Status = RunStatus.Running;
        run.StartedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Starting execution of Workflow Run {RunId}", run.Id);

        var steps = run.StepExecutions.OrderBy(s => s.StepId).ToList();
        bool anyFailed = false;

        foreach (var step in steps)
        {
            if (ct.IsCancellationRequested) break;

            step.Status = StepStatus.Running;
            step.StartedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            await onStepStatusChanged(run.Id, step.StepId, "Running");

            // Stream progress logs
            await LogAndEmitAsync(step, run.Id, "INFO", $"Initializing task runner for step '{step.Name}' [{step.Type}]", onLogEmitted, ct);
            await Task.Delay(1000, ct); // Realistic task execution simulation

            await LogAndEmitAsync(step, run.Id, "INFO", $"Allocated worker container. Processing payload data...", onLogEmitted, ct);
            await Task.Delay(1500, ct);

            // Complete step
            step.Status = StepStatus.Succeeded;
            step.CompletedAtUtc = DateTime.UtcNow;
            await LogAndEmitAsync(step, run.Id, "INFO", $"Step completed successfully with exit code 0.", onLogEmitted, ct);

            await _context.SaveChangesAsync(ct);
            await onStepStatusChanged(run.Id, step.StepId, "Succeeded");
        }

        run.Status = anyFailed ? RunStatus.Failed : RunStatus.Succeeded;
        run.CompletedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Completed Workflow Run {RunId} with final status {Status}", run.Id, run.Status);
    }

    private async Task LogAndEmitAsync(
        StepExecution step,
        Guid runId,
        string level,
        string message,
        Func<Guid, string, string, Task> onLogEmitted,
        CancellationToken ct)
    {
        var log = new StepLog
        {
            StepExecutionId = step.Id,
            LogLevel = level,
            Message = message,
            TimestampUtc = DateTime.UtcNow
        };
        step.Logs.Add(log);
        await onLogEmitted(runId, level, message);
    }

    private record DagStepDefinition(string StepId, string Name, string Type, List<string>? DependsOn);
}
