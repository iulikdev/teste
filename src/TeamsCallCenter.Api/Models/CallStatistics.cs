namespace TeamsCallCenter.Api.Models;

public class CallStatistics
{
    public int ActiveCalls { get; set; }
    public int CallsInQueue { get; set; }
    public int TotalCallsToday { get; set; }
    public int AnsweredCalls { get; set; }
    public int MissedCalls { get; set; }
    public TimeSpan AverageWaitTime { get; set; }
    public TimeSpan AverageCallDuration { get; set; }
    public int AvailableAgents { get; set; }
    public int BusyAgents { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
