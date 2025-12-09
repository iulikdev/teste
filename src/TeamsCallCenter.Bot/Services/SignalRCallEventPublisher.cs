using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using TeamsCallCenter.Bot.Configuration;
using TeamsCallCenter.Shared.Models;

namespace TeamsCallCenter.Bot.Services;

public class SignalRCallEventPublisher : ICallEventPublisher, IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<SignalRCallEventPublisher> _logger;

    public SignalRCallEventPublisher(
        IOptions<ApiConfiguration> config,
        ILogger<SignalRCallEventPublisher> logger)
    {
        _logger = logger;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(config.Value.SignalRHubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.Closed += async (error) =>
        {
            _logger.LogWarning(error, "SignalR connection closed. Reconnecting...");
            await Task.Delay(TimeSpan.FromSeconds(5));
            await ConnectAsync();
        };
    }

    public async Task ConnectAsync()
    {
        try
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
                _logger.LogInformation("Connected to SignalR hub");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to SignalR hub");
        }
    }

    public async Task PublishCallStartedAsync(CallInfo callInfo)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("CallStarted", callInfo);
        _logger.LogInformation("Published CallStarted for {CallId}", callInfo.CallId);
    }

    public async Task PublishCallEndedAsync(string callId)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("CallEnded", callId);
        _logger.LogInformation("Published CallEnded for {CallId}", callId);
    }

    public async Task PublishCallUpdatedAsync(CallInfo callInfo)
    {
        await EnsureConnectedAsync();
        await _hubConnection.InvokeAsync("CallUpdated", callInfo);
    }

    public async Task PublishAudioReceivedAsync(string callId, byte[] audioData, long timestamp)
    {
        // Audio data is not sent via SignalR - too much data
        // Instead, store locally or send to a processing service
        await Task.CompletedTask;
    }

    private async Task EnsureConnectedAsync()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            await ConnectAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _hubConnection.DisposeAsync();
    }
}
