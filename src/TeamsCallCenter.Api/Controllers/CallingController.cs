using Microsoft.AspNetCore.Mvc;

namespace TeamsCallCenter.Api.Controllers;

[ApiController]
[Route("api/calling")]
public class CallingController : ControllerBase
{
    private readonly ILogger<CallingController> _logger;

    public CallingController(ILogger<CallingController> logger)
    {
        _logger = logger;
    }

    // This endpoint receives notifications from Microsoft Graph
    // The Bot service handles these directly, but we expose this for routing
    [HttpPost]
    public IActionResult HandleNotification()
    {
        _logger.LogInformation("Received calling notification");

        // In a real implementation, this would be handled by the Bot service
        // This controller is here for the webhook endpoint

        return Ok();
    }
}
