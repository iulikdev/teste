using Microsoft.Graph.Communications.Calls;
using TeamsCallCenter.Models;

namespace TeamsCallCenter.Bot.Services;

public interface IBotService
{
    Task InitializeAsync();
    Task<ICall?> GetCallAsync(string callId);
    IEnumerable<CallInfo> GetActiveCalls();
    Task EndCallAsync(string callId);
}
