using TeamsCallCenter.Api;
using TeamsCallCenter.Api.Bot.Audio;
using TeamsCallCenter.Api.Bot.Configuration;
using TeamsCallCenter.Api.Bot.Services;
using TeamsCallCenter.Api.Hubs;
using TeamsCallCenter.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR
builder.Services.AddSignalR();

// Call state service (in-memory for now, could be Redis/SQL in production)
builder.Services.AddSingleton<ICallStateService, InMemoryCallStateService>();

// Bot Configuration
builder.Services.Configure<BotConfiguration>(builder.Configuration.GetSection("Bot"));
builder.Services.Configure<RecordingConfiguration>(builder.Configuration.GetSection("Recording"));

// Bot Services
builder.Services.AddSingleton<ICallEventPublisher, DirectCallEventPublisher>();
builder.Services.AddSingleton<IAudioRecordingService, AudioRecordingService>();
builder.Services.AddSingleton<IBotService, BotService>();

// CORS for Svelte dashboard
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dashboard", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",  // Svelte dev server
                "https://localhost:5173",
                "https://*.teams.microsoft.com" // Teams
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("Dashboard");
app.UseAuthorization();

app.MapControllers();
app.MapHub<CallsHub>("/hubs/calls");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

// Initialize Bot Service on startup
var botService = app.Services.GetRequiredService<IBotService>();
await botService.InitializeAsync();

app.Run();
