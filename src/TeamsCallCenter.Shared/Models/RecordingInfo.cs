namespace TeamsCallCenter.Shared.Models;

public class RecordingInfo
{
    public string RecordingId { get; set; } = Guid.NewGuid().ToString();
    public string CallId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration => EndTime.HasValue
        ? EndTime.Value - StartTime
        : TimeSpan.Zero;
    public long FileSizeBytes { get; set; }

    // Call metadata
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string CallerDisplayName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public CallDirection Direction { get; set; }
    public CallStatus FinalStatus { get; set; }

    // Audio format info
    public int SampleRate { get; set; } = 16000;
    public int BitsPerSample { get; set; } = 16;
    public int Channels { get; set; } = 1;
}
