using System.Collections.Concurrent;
using TeamsCallCenter.Api.Models;

namespace TeamsCallCenter.Api.Services;

public class InMemoryCallStateService : ICallStateService
{
    private readonly ConcurrentDictionary<string, CallInfo> _activeCalls = new();
    private int _totalCallsToday;
    private int _answeredCalls;
    private int _missedCalls;
    private readonly List<TimeSpan> _callDurations = new();
    private readonly object _statsLock = new();

    public void AddCall(CallInfo call)
    {
        _activeCalls[call.CallId] = call;
        Interlocked.Increment(ref _totalCallsToday);
    }

    public void UpdateCall(CallInfo call)
    {
        _activeCalls[call.CallId] = call;

        if (call.Status == CallStatus.Connected)
        {
            Interlocked.Increment(ref _answeredCalls);
        }
    }

    public void RemoveCall(string callId)
    {
        if (_activeCalls.TryRemove(callId, out var call))
        {
            call.EndTime = DateTime.UtcNow;

            lock (_statsLock)
            {
                _callDurations.Add(call.Duration);
            }
        }
    }

    public CallInfo? GetCall(string callId)
    {
        return _activeCalls.TryGetValue(callId, out var call) ? call : null;
    }

    public IEnumerable<CallInfo> GetActiveCalls()
    {
        return _activeCalls.Values.ToList();
    }

    public CallStatistics GetStatistics()
    {
        var activeCalls = _activeCalls.Values.ToList();

        TimeSpan avgDuration;
        lock (_statsLock)
        {
            avgDuration = _callDurations.Count > 0
                ? TimeSpan.FromTicks((long)_callDurations.Average(d => d.Ticks))
                : TimeSpan.Zero;
        }

        return new CallStatistics
        {
            ActiveCalls = activeCalls.Count(c => c.Status == CallStatus.Connected),
            CallsInQueue = activeCalls.Count(c => c.Status == CallStatus.InQueue || c.Status == CallStatus.Ringing),
            TotalCallsToday = _totalCallsToday,
            AnsweredCalls = _answeredCalls,
            MissedCalls = _missedCalls,
            AverageCallDuration = avgDuration,
            AverageWaitTime = TimeSpan.FromSeconds(30), // TODO: Calculate from actual data
            AvailableAgents = 5, // TODO: Get from agent service
            BusyAgents = activeCalls.Select(c => c.AgentId).Distinct().Count(),
            LastUpdated = DateTime.UtcNow
        };
    }
}
