using TeamsCallCenter.Api.Models;

namespace TeamsCallCenter.Api.Bot.Services;

public interface ICallEventPublisher
{
    Task PublishCallStartedAsync(CallInfo callInfo);
    Task PublishCallEndedAsync(string callId);
    Task PublishCallUpdatedAsync(CallInfo callInfo);
    Task PublishAudioReceivedAsync(string callId, byte[] audioData, long timestamp);
}
