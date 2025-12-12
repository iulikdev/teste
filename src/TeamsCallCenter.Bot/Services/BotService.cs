using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Calls;
using Microsoft.Graph.Communications.Calls.Media;
using Microsoft.Graph.Communications.Client;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Graph.Communications.Resources;
using Microsoft.Skype.Bots.Media;
using TeamsCallCenter.Bot.Audio;
using TeamsCallCenter.Bot.Configuration;
using TeamsCallCenter.Bot.Media;
using TeamsCallCenter.Shared.Models;

namespace TeamsCallCenter.Bot.Services;

public class BotService : IBotService, IDisposable
{
    private readonly BotConfiguration _config;
    private readonly ILogger<BotService> _logger;
    private readonly IGraphLogger _graphLogger;
    private readonly ICallEventPublisher _callEventPublisher;
    private readonly IAudioRecordingService? _recordingService;
    private ICommunicationsClient? _client;
    private readonly ConcurrentDictionary<string, CallHandler> _callHandlers = new();

    public BotService(
        IOptions<BotConfiguration> config,
        ILogger<BotService> logger,
        ICallEventPublisher callEventPublisher,
        IAudioRecordingService? recordingService = null)
    {
        _config = config.Value;
        _logger = logger;
        _callEventPublisher = callEventPublisher;
        _recordingService = recordingService;
        _graphLogger = new GraphLogger("BotService");
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing Bot Service...");

        var authProvider = new AuthenticationProvider(
            _config.AppId,
            _config.AppSecret,
            _graphLogger);

        var mediaPlatformSettings = new MediaPlatformSettings
        {
            MediaPlatformInstanceSettings = new MediaPlatformInstanceSettings
            {
                CertificateThumbprint = _config.CertificateThumbprint,
                InstanceInternalPort = _config.InstanceInternalPort,
                InstancePublicIPAddress = System.Net.IPAddress.Any,
                InstancePublicPort = _config.InstancePublicPort,
                ServiceFqdn = _config.ServiceDnsName
            },
            ApplicationId = _config.AppId
        };

        var builder = new CommunicationsClientBuilder(
            _config.BotName,
            _config.AppId,
            _graphLogger);

        builder
            .SetAuthenticationProvider(authProvider)
            .SetMediaPlatformSettings(mediaPlatformSettings)
            .SetNotificationUrl(new Uri($"https://{_config.ServiceCname}/api/calling"));

        _client = builder.Build();

        _client.Calls().OnIncoming += OnIncomingCall;
        _client.Calls().OnUpdated += OnCallUpdated;

        _logger.LogInformation("Bot Service initialized successfully");
    }

    private void OnIncomingCall(ICallCollection sender, CollectionEventArgs<ICall> args)
    {
        var call = args.AddedResources.FirstOrDefault();
        if (call == null) return;

        _logger.LogInformation("Incoming call: {CallId}", call.Id);

        Task.Run(async () =>
        {
            try
            {
                var audioSettings = CreateAudioSocketSettings();
                var mediaSession = call.CreateMediaSession(audioSettings);
                await call.AnswerAsync(mediaSession).ConfigureAwait(false);

                var handler = new CallHandler(call, _graphLogger, _callEventPublisher, _recordingService);
                _callHandlers[call.Id] = handler;

                await _callEventPublisher.PublishCallStartedAsync(new CallInfo
                {
                    CallId = call.Id,
                    Status = CallStatus.Connected,
                    StartTime = DateTime.UtcNow,
                    Direction = CallDirection.Inbound
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error answering call {CallId}", call.Id);
            }
        });
    }

    private void OnCallUpdated(ICallCollection sender, CollectionEventArgs<ICall> args)
    {
        foreach (var call in args.RemovedResources)
        {
            _logger.LogInformation("Call ended: {CallId}", call.Id);

            if (_callHandlers.TryRemove(call.Id, out var handler))
            {
                handler.Dispose();

                Task.Run(async () =>
                {
                    await _callEventPublisher.PublishCallEndedAsync(call.Id);
                });
            }
        }
    }

    private AudioSocketSettings CreateAudioSocketSettings()
    {
        return new AudioSocketSettings
        {
            StreamDirections = StreamDirection.Recvonly,
            SupportedAudioFormat = AudioFormat.Pcm16K
        };
    }

    public Task<ICall?> GetCallAsync(string callId)
    {
        return Task.FromResult(_client?.Calls()[callId]);
    }

    public IEnumerable<CallInfo> GetActiveCalls()
    {
        return _callHandlers.Values.Select(h => h.GetCallInfo());
    }

    public async Task EndCallAsync(string callId)
    {
        var call = _client?.Calls()[callId];
        if (call != null)
        {
            await call.DeleteAsync().ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        foreach (var handler in _callHandlers.Values)
        {
            handler.Dispose();
        }
        _callHandlers.Clear();

        (_client as IDisposable)?.Dispose();
    }
}
