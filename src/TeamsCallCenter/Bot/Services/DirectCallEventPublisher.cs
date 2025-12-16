using Microsoft.AspNetCore.SignalR;
using TeamsCallCenter.Hubs;
using TeamsCallCenter.Models;

namespace TeamsCallCenter.Bot.Services;

/// <summary>
/// Publishes call events directly to SignalR clients using IHubContext.
/// This is used when Bot and API are in the same process.
/// </summary>
public class DirectCallEventPublisher : ICallEventPublisher
{
    private readonly IHubContext<CallsHub> _hubContext;
    private readonly ICallStateService _callStateService;
    private readonly ILogger<DirectCallEventPublisher> _logger;

    public DirectCallEventPublisher(
        IHubContext<CallsHub> hubContext,
        ICallStateService callStateService,
        ILogger<DirectCallEventPublisher> logger)
    {
        _hubContext = hubContext;
        _callStateService = callStateService;
        _logger = logger;
    }

    public async Task PublishCallStartedAsync(CallInfo callInfo)
    {
        _logger.LogInformation("Publishing CallStarted for {CallId}", callInfo.CallId);
        _callStateService.AddCall(callInfo);

        await _hubContext.Clients.All.SendAsync("CallStarted", callInfo);
        await BroadcastStatisticsAsync();
    }

    public async Task PublishCallEndedAsync(string callId)
    {
        _logger.LogInformation("Publishing CallEnded for {CallId}", callId);
        _callStateService.RemoveCall(callId);

        await _hubContext.Clients.All.SendAsync("CallEnded", callId);
        await BroadcastStatisticsAsync();
    }

    public async Task PublishCallUpdatedAsync(CallInfo callInfo)
    {
        _logger.LogInformation("Publishing CallUpdated for {CallId}", callInfo.CallId);
        _callStateService.UpdateCall(callInfo);

        await _hubContext.Clients.All.SendAsync("CallUpdated", callInfo);
        await BroadcastStatisticsAsync();
    }

    public Task PublishAudioReceivedAsync(string callId, byte[] audioData, long timestamp)
    {
        // Audio data is not sent via SignalR - too much data
        // Instead, store locally or send to a processing service
        return Task.CompletedTask;
    }

    private async Task BroadcastStatisticsAsync()
    {
        var stats = _callStateService.GetStatistics();
        await _hubContext.Clients.All.SendAsync("StatisticsUpdated", stats);
    }
}
