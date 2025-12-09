using TeamsCallCenter.Bot.Audio;
using TeamsCallCenter.Bot.Configuration;
using TeamsCallCenter.Bot.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Services.Configure<BotConfiguration>(builder.Configuration.GetSection("Bot"));
builder.Services.Configure<ApiConfiguration>(builder.Configuration.GetSection("Api"));
builder.Services.Configure<RecordingConfiguration>(builder.Configuration.GetSection("Recording"));

// Services
builder.Services.AddSingleton<ICallEventPublisher, SignalRCallEventPublisher>();
builder.Services.AddSingleton<IAudioRecordingService, AudioRecordingService>();
builder.Services.AddSingleton<IBotService, BotService>();

// Hosted service to initialize bot
builder.Services.AddHostedService<BotHostedService>();

var host = builder.Build();
host.Run();

public class BotHostedService : IHostedService
{
    private readonly IBotService _botService;
    private readonly ICallEventPublisher _eventPublisher;
    private readonly ILogger<BotHostedService> _logger;

    public BotHostedService(
        IBotService botService,
        ICallEventPublisher eventPublisher,
        ILogger<BotHostedService> logger)
    {
        _botService = botService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Teams Call Center Bot...");

        if (_eventPublisher is SignalRCallEventPublisher signalR)
        {
            await signalR.ConnectAsync();
        }

        await _botService.InitializeAsync();

        _logger.LogInformation("Teams Call Center Bot started");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Teams Call Center Bot...");
        return Task.CompletedTask;
    }
}
