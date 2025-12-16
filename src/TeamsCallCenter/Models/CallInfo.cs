namespace TeamsCallCenter.Models;

public class CallInfo
{
    public string CallId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public CallStatus Status { get; set; } = CallStatus.Ringing;
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration => EndTime.HasValue
        ? EndTime.Value - StartTime
        : DateTime.UtcNow - StartTime;
    public CallDirection Direction { get; set; } = CallDirection.Inbound;
}

public enum CallStatus
{
    Ringing,
    InQueue,
    Connected,
    OnHold,
    Transferring,
    Ended
}

public enum CallDirection
{
    Inbound,
    Outbound
}
