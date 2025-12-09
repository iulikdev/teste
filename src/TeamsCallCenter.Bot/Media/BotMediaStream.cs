using System.Runtime.InteropServices;
using Microsoft.Graph.Communications.Calls;
using Microsoft.Graph.Communications.Calls.Media;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Skype.Bots.Media;
using TeamsCallCenter.Bot.Audio;
using TeamsCallCenter.Bot.Services;
using TeamsCallCenter.Shared.Models;

namespace TeamsCallCenter.Bot.Media;

public class BotMediaStream : IDisposable
{
    private readonly string _callId;
    private readonly IMediaSession _mediaSession;
    private readonly IGraphLogger _logger;
    private readonly ICallEventPublisher _eventPublisher;
    private readonly IAudioSocket? _audioSocket;
    private readonly CallRecordingSession? _recordingSession;
    private readonly object _lock = new();
    private bool _disposed;

    public BotMediaStream(
        string callId,
        IMediaSession mediaSession,
        IGraphLogger logger,
        ICallEventPublisher eventPublisher,
        CallRecordingSession? recordingSession = null)
    {
        _callId = callId;
        _mediaSession = mediaSession;
        _logger = logger;
        _eventPublisher = eventPublisher;
        _recordingSession = recordingSession;

        // IMediaSession may be ILocalMediaSession which has AudioSocket
        if (mediaSession is ILocalMediaSession localSession)
        {
            _audioSocket = localSession.AudioSocket;
            if (_audioSocket != null)
            {
                _audioSocket.AudioMediaReceived += OnAudioMediaReceived;
            }
        }
    }

    private void OnAudioMediaReceived(object sender, AudioMediaReceivedEventArgs e)
    {
        try
        {
            if (_recordingSession != null)
            {
                // Copy audio data before the buffer is disposed
                var audioData = new byte[e.Buffer.Length];
                Marshal.Copy(e.Buffer.Data, audioData, 0, (int)e.Buffer.Length);

                // Write to WAV file
                _recordingSession.WriteAudio(audioData);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error processing audio for call {_callId}");
        }
        finally
        {
            // IMPORTANT: Always dispose the buffer
            e.Buffer.Dispose();
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed) return;
            _disposed = true;
        }

        if (_audioSocket != null)
        {
            _audioSocket.AudioMediaReceived -= OnAudioMediaReceived;
        }
    }
}
