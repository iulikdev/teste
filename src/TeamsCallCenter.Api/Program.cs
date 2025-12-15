using TeamsCallCenter.Api;
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

app.Run();
