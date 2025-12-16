using TeamsCallCenter.Models;

namespace TeamsCallCenter;

public interface ICallStateService
{
    void AddCall(CallInfo call);
    void UpdateCall(CallInfo call);
    void RemoveCall(string callId);
    CallInfo? GetCall(string callId);
    IEnumerable<CallInfo> GetActiveCalls();
    CallStatistics GetStatistics();
}
