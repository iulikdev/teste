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
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        _logger.LogWarning("=== TEAMS NOTIFICATION RECEIVED ===");
        _logger.LogWarning("[{Timestamp}] POST /api/calling from IP: {ClientIp}", timestamp, clientIp);
        _logger.LogWarning("User-Agent: {UserAgent}", userAgent);

        // Log all headers
        foreach (var header in Request.Headers)
        {
            _logger.LogInformation("Header: {Key} = {Value}", header.Key, header.Value);
        }

        try
        {
            // Read the request body
            using var reader = new StreamReader(Request.Body);
            var content = await reader.ReadToEndAsync();

            _logger.LogWarning("Body length: {Length} chars", content.Length);
            _logger.LogWarning("Body content: {Content}", content);

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
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        _logger.LogWarning("=== TEAMS CALLBACK RECEIVED ===");
        _logger.LogWarning("[{Timestamp}] POST /api/calling/callback from IP: {ClientIp}", timestamp, clientIp);

        // Log all headers
        foreach (var header in Request.Headers)
        {
            _logger.LogInformation("Header: {Key} = {Value}", header.Key, header.Value);
        }

        try
        {
            using var reader = new StreamReader(Request.Body);
            var content = await reader.ReadToEndAsync();

            _logger.LogWarning("Body length: {Length} chars", content.Length);
            _logger.LogWarning("Body content: {Content}", content);

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
