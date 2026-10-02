using Microsoft.AspNetCore.SignalR;

namespace FlowForge.Api.Hubs;

public class WorkflowHub : Hub
{
    public async Task SubscribeToRun(string runId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, runId);
    }

    public async Task UnsubscribeFromRun(string runId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, runId);
    }
}
