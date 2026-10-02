using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowForge.Core.Entities;
using FlowForge.Infrastructure.Data;

namespace FlowForge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class WorkflowsController : ControllerBase
{
    private readonly FlowForgeDbContext _context;

    public WorkflowsController(FlowForgeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var workflows = await _context.WorkflowDefinitions
            .Include(w => w.Runs)
            .OrderByDescending(w => w.CreatedAtUtc)
            .ToListAsync(ct);

        var dtos = workflows.Select(w => new
        {
            w.Id,
            w.Name,
            w.Description,
            w.Category,
            w.StepsJson,
            TotalRuns = w.Runs.Count,
            LastRunStatus = w.Runs.OrderByDescending(r => r.QueuedAtUtc).FirstOrDefault()?.Status.ToString(),
            w.CreatedAtUtc
        });

        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var w = await _context.WorkflowDefinitions
            .Include(x => x.Runs)
                .ThenInclude(r => r.StepExecutions)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (w == null) return NotFound();
        return Ok(w);
    }
}
