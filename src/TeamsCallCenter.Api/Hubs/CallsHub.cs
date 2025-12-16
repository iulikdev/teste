using Microsoft.AspNetCore.SignalR;
using TeamsCallCenter.Api.Models;

namespace TeamsCallCenter.Api.Hubs;

public class CallsHub : Hub
{
    private readonly ILogger<CallsHub> _logger;
    private readonly ICallStateService _callStateService;

    public CallsHub(ILogger<CallsHub> logger, ICallStateService callStateService)
    {
        _logger = logger;
        _callStateService = callStateService;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);

        // Send current state to newly connected client
        var activeCalls = _callStateService.GetActiveCalls();
        var stats = _callStateService.GetStatistics();

        await Clients.Caller.SendAsync("InitialState", new
        {
            Calls = activeCalls,
            Statistics = stats
        });

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    // Called by the Bot service when a call starts
    public async Task CallStarted(CallInfo callInfo)
    {
        _logger.LogInformation("Call started: {CallId}", callInfo.CallId);
        _callStateService.AddCall(callInfo);

        await Clients.All.SendAsync("CallStarted", callInfo);
        await BroadcastStatistics();
    }

    // Called by the Bot service when a call ends
    public async Task CallEnded(string callId)
    {
        _logger.LogInformation("Call ended: {CallId}", callId);
        _callStateService.RemoveCall(callId);

        await Clients.All.SendAsync("CallEnded", callId);
        await BroadcastStatistics();
    }

    // Called by the Bot service when a call is updated
    public async Task CallUpdated(CallInfo callInfo)
    {
        _logger.LogInformation("Call updated: {CallId}", callInfo.CallId);
        _callStateService.UpdateCall(callInfo);

        await Clients.All.SendAsync("CallUpdated", callInfo);
        await BroadcastStatistics();
    }

    private async Task BroadcastStatistics()
    {
        var stats = _callStateService.GetStatistics();
        await Clients.All.SendAsync("StatisticsUpdated", stats);
    }
}
