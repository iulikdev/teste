using TeamsCallCenter.Models;

namespace TeamsCallCenter.Bot.Services;

public interface ICallEventPublisher
{
    Task PublishCallStartedAsync(CallInfo callInfo);
    Task PublishCallEndedAsync(string callId);
    Task PublishCallUpdatedAsync(CallInfo callInfo);
    Task PublishAudioReceivedAsync(string callId, byte[] audioData, long timestamp);
}
