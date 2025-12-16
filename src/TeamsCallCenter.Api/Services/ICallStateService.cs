using TeamsCallCenter.Api.Models;

namespace TeamsCallCenter.Api;

public interface ICallStateService
{
    void AddCall(CallInfo call);
    void UpdateCall(CallInfo call);
    void RemoveCall(string callId);
    CallInfo? GetCall(string callId);
    IEnumerable<CallInfo> GetActiveCalls();
    CallStatistics GetStatistics();
}
