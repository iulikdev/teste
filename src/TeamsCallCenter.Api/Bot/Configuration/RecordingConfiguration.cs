namespace TeamsCallCenter.Api.Bot.Configuration;

public class RecordingConfiguration
{
    public string RecordingPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "Recordings");
    public bool Enabled { get; set; } = true;
}
