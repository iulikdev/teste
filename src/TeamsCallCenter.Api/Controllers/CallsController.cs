using Microsoft.AspNetCore.Mvc;
using TeamsCallCenter.Shared.Models;

namespace TeamsCallCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CallsController : ControllerBase
{
    private readonly ICallStateService _callStateService;
    private readonly ILogger<CallsController> _logger;

    public CallsController(ICallStateService callStateService, ILogger<CallsController> logger)
    {
        _callStateService = callStateService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<CallInfo>> GetActiveCalls()
    {
        return Ok(_callStateService.GetActiveCalls());
    }

    [HttpGet("{callId}")]
    public ActionResult<CallInfo> GetCall(string callId)
    {
        var call = _callStateService.GetCall(callId);
        if (call == null)
        {
            return NotFound();
        }
        return Ok(call);
    }

    [HttpGet("statistics")]
    public ActionResult<CallStatistics> GetStatistics()
    {
        return Ok(_callStateService.GetStatistics());
    }
}
