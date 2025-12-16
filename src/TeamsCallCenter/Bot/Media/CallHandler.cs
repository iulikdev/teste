using Microsoft.Graph.Communications.Calls;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Graph.Communications.Resources;
using Microsoft.Graph.Models;
using TeamsCallCenter.Bot.Audio;
using TeamsCallCenter.Bot.Services;
using TeamsCallCenter.Models;

namespace TeamsCallCenter.Bot.Media;

public class CallHandler : IDisposable
{
    private readonly ICall _call;
    private readonly IGraphLogger _logger;
    private readonly ICallEventPublisher _eventPublisher;
    private readonly IAudioRecordingService? _recordingService;
    private readonly BotMediaStream _mediaStream;
    private readonly DateTime _startTime;

    public CallHandler(
        ICall call,
        IGraphLogger logger,
        ICallEventPublisher eventPublisher,
        IAudioRecordingService? recordingService = null)
    {
        _call = call;
        _logger = logger;
        _eventPublisher = eventPublisher;
        _recordingService = recordingService;
        _startTime = DateTime.UtcNow;

        // Start recording if service is available
        CallRecordingSession? recordingSession = null;
        if (_recordingService != null && call.Resource.Id != null)
        {
            var callInfo = GetCallInfo();
            recordingSession = _recordingService.StartRecording(call.Resource.Id, callInfo);
        }

        _mediaStream = new BotMediaStream(
            call.Resource.Id ?? string.Empty,
            call.MediaSession,
            logger,
            eventPublisher,
            recordingSession);

        _call.OnUpdated += OnCallUpdated;
    }

    public string CallId => _call.Resource.Id ?? string.Empty;

    public CallInfo GetCallInfo()
    {
        var resource = _call.Resource;

        return new CallInfo
        {
            CallId = resource.Id ?? string.Empty,
            Status = MapCallState(resource.State),
            StartTime = _startTime,
            Direction = resource.Direction == Microsoft.Graph.Models.CallDirection.Incoming
                ? Models.CallDirection.Inbound
                : Models.CallDirection.Outbound,
            DisplayName = resource.Source?.Identity?.User?.DisplayName
                ?? resource.Source?.Identity?.Application?.DisplayName
                ?? "Unknown"
        };
    }

    private void OnCallUpdated(ICall sender, ResourceEventArgs<Call> args)
    {
        _logger.Info($"Call {_call.Id} updated: {args.NewResource.State}");

        Task.Run(async () =>
        {
            await _eventPublisher.PublishCallUpdatedAsync(GetCallInfo());
        });
    }

    private static Models.CallStatus MapCallState(CallState? state)
    {
        return state switch
        {
            CallState.Incoming => Models.CallStatus.Ringing,
            CallState.Establishing => Models.CallStatus.Ringing,
            CallState.Established => Models.CallStatus.Connected,
            CallState.Hold => Models.CallStatus.OnHold,
            CallState.Transferring => Models.CallStatus.Transferring,
            CallState.TransferAccepted => Models.CallStatus.Transferring,
            CallState.Terminated => Models.CallStatus.Ended,
            _ => Models.CallStatus.Connected
        };
    }

    public void Dispose()
    {
        _call.OnUpdated -= OnCallUpdated;
        _mediaStream.Dispose();

        // Stop recording when call ends
        if (_call.Resource.Id != null)
        {
            _recordingService?.StopRecording(_call.Resource.Id);
        }
    }
}
