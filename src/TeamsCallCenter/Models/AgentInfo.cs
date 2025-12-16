namespace TeamsCallCenter.Models;

public class AgentInfo
{
    public string AgentId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public AgentStatus Status { get; set; } = AgentStatus.Offline;
    public string? CurrentCallId { get; set; }
    public int CallsHandledToday { get; set; }
    public TimeSpan TotalTalkTime { get; set; }
}

public enum AgentStatus
{
    Available,
    Busy,
    Away,
    Offline
}
