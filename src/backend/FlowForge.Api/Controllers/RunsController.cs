using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowForge.Infrastructure.Data;
using FlowForge.Infrastructure.Engine;

namespace FlowForge.Api.Controllers;

public record TriggerWorkflowRunRequest(
    string TriggeredBy,
    string InputPayload
);

[ApiController]
[Route("api/v1/[controller]")]
public class RunsController : ControllerBase
{
    private readonly FlowForgeDbContext _context;
    private readonly IWorkflowEngine _engine;

    public RunsController(FlowForgeDbContext context, IWorkflowEngine engine)
    {
        _context = context;
        _engine = engine;
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetRunsHistory([FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var runs = await _context.WorkflowRuns
            .Include(r => r.WorkflowDefinition)
            .Include(r => r.StepExecutions)
            .OrderByDescending(r => r.QueuedAtUtc)
            .Take(limit)
            .ToListAsync(ct);

        var dtos = runs.Select(r => new
        {
            r.Id,
            r.WorkflowDefinitionId,
            WorkflowName = r.WorkflowDefinition?.Name ?? "Unknown",
            Status = r.Status.ToString(),
            r.TriggeredBy,
            r.QueuedAtUtc,
            r.StartedAtUtc,
            r.CompletedAtUtc,
            DurationSeconds = (r.CompletedAtUtc.HasValue && r.StartedAtUtc.HasValue) 
                ? (int)(r.CompletedAtUtc.Value - r.StartedAtUtc.Value).TotalSeconds 
                : 0,
            StepsCount = r.StepExecutions.Count,
            SucceededSteps = r.StepExecutions.Count(s => s.Status == Core.Enums.StepStatus.Succeeded)
        });

        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRunDetails(Guid id, CancellationToken ct)
    {
        var run = await _context.WorkflowRuns
            .Include(r => r.WorkflowDefinition)
            .Include(r => r.StepExecutions)
                .ThenInclude(s => s.Logs)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (run == null) return NotFound();

        return Ok(new
        {
            run.Id,
            run.WorkflowDefinitionId,
            WorkflowName = run.WorkflowDefinition?.Name,
            Status = run.Status.ToString(),
            run.TriggeredBy,
            run.InputPayloadJson,
            run.QueuedAtUtc,
            run.StartedAtUtc,
            run.CompletedAtUtc,
            Steps = run.StepExecutions.Select(s => new
            {
                s.Id,
                s.StepId,
                s.Name,
                s.Type,
                Status = s.Status.ToString(),
                s.StartedAtUtc,
                s.CompletedAtUtc,
                Logs = s.Logs.OrderBy(l => l.TimestampUtc).Select(l => new
                {
                    l.LogLevel,
                    l.Message,
                    l.TimestampUtc
                })
            })
        });
    }

    [HttpPost("trigger/{workflowId:guid}")]
    public async Task<IActionResult> TriggerRun(Guid workflowId, [FromBody] TriggerWorkflowRunRequest request, CancellationToken ct)
    {
        var run = await _engine.QueueRunAsync(workflowId, request.TriggeredBy ?? "ManualUser", request.InputPayload ?? "{}", ct);
        return Ok(new
        {
            runId = run.Id,
            status = run.Status.ToString(),
            message = "Pipeline successfully queued for distributed worker execution."
        });
    }
}
