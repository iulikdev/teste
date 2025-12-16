using Microsoft.AspNetCore.Mvc;
using TeamsCallCenter.Bot.Services;

namespace TeamsCallCenter.Controllers;

[ApiController]
[Route("api/calling")]
public class CallingController : ControllerBase
{
    private readonly ILogger<CallingController> _logger;
    private readonly IBotService _botService;

    public CallingController(ILogger<CallingController> logger, IBotService botService)
    {
        _logger = logger;
        _botService = botService;
    }

    /// <summary>
    /// Receives notifications from Microsoft Graph Communications API.
    /// This endpoint handles incoming call notifications and call state changes.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> HandleNotification()
    {
        _logger.LogInformation("Received calling notification");

        try
        {
            // Read the request body
            using var reader = new StreamReader(Request.Body);
            var content = await reader.ReadToEndAsync();

            _logger.LogDebug("Notification content: {Content}", content);

            // The Graph Communications SDK handles notifications through its internal mechanism
            // when properly configured. The notifications are processed by the ICommunicationsClient
            // that was set up in BotService.InitializeAsync()

            // For now, we acknowledge receipt. The SDK's callback URLs handle the actual processing.
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing calling notification");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Callback endpoint for call notifications.
    /// </summary>
    [HttpPost("callback")]
    public async Task<IActionResult> HandleCallback()
    {
        _logger.LogInformation("Received calling callback");

        try
        {
            using var reader = new StreamReader(Request.Body);
            var content = await reader.ReadToEndAsync();

            _logger.LogDebug("Callback content: {Content}", content);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing calling callback");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Get list of active calls from the bot.
    /// </summary>
    [HttpGet("calls")]
    public IActionResult GetActiveCalls()
    {
        var calls = _botService.GetActiveCalls();
        return Ok(calls);
    }

    /// <summary>
    /// End a specific call.
    /// </summary>
    [HttpDelete("calls/{callId}")]
    public async Task<IActionResult> EndCall(string callId)
    {
        try
        {
            await _botService.EndCallAsync(callId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending call {CallId}", callId);
            return StatusCode(500);
        }
    }
}
