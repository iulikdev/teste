namespace TeamsCallCenter.Bot.Configuration;

public class BotConfiguration
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string BotName { get; set; } = string.Empty;
    public string ServiceDnsName { get; set; } = string.Empty;
    public string ServiceCname { get; set; } = string.Empty;
    public string CertificateThumbprint { get; set; } = string.Empty;
    public int InstancePublicPort { get; set; } = 443;
    public int InstanceInternalPort { get; set; } = 8445;
    public int CallSignalingPort { get; set; } = 9441;
    public string PlaceCallEndpointUrl { get; set; } = "https://graph.microsoft.com/v1.0";

    public Uri CallControlBaseUrl => new Uri($"https://{ServiceCname}:{InstancePublicPort}/api/calling");
    public Uri MediaPlatformInstanceUri => new Uri($"https://{ServiceDnsName}:{InstancePublicPort}/");
}
