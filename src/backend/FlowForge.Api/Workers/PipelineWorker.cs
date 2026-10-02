using Microsoft.AspNetCore.SignalR;
using FlowForge.Api.Hubs;
using FlowForge.Infrastructure.Engine;

namespace FlowForge.Api.Workers;

public class PipelineWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<WorkflowHub> _hubContext;
    private readonly ILogger<PipelineWorker> _logger;

    public PipelineWorker(
        IServiceProvider serviceProvider,
        IHubContext<WorkflowHub> hubContext,
        ILogger<PipelineWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Pipeline background execution worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var engine = scope.ServiceProvider.GetRequiredService<IWorkflowEngine>();

                await engine.ExecuteNextPendingRunAsync(
                    onStepStatusChanged: async (runId, stepId, status) =>
                    {
                        await _hubContext.Clients.Group(runId.ToString()).SendAsync("StepStatusChanged", new
                        {
                            runId,
                            stepId,
                            status
                        }, stoppingToken);
                    },
                    onLogEmitted: async (runId, level, message) =>
                    {
                        await _hubContext.Clients.Group(runId.ToString()).SendAsync("LogEmitted", new
                        {
                            runId,
                            level,
                            message,
                            timestamp = DateTime.UtcNow
                        }, stoppingToken);
                    },
                    ct: stoppingToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during pipeline execution iteration.");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
