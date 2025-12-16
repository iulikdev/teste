using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TeamsCallCenter.Bot.Configuration;
using TeamsCallCenter.Models;

namespace TeamsCallCenter.Bot.Audio;

public interface IAudioRecordingService
{
    CallRecordingSession StartRecording(string callId, CallInfo callInfo);
    void StopRecording(string callId);
    RecordingInfo? GetRecordingInfo(string callId);
}

public class AudioRecordingService : IAudioRecordingService, IDisposable
{
    private readonly RecordingConfiguration _config;
    private readonly ILogger<AudioRecordingService> _logger;
    private readonly ConcurrentDictionary<string, CallRecordingSession> _activeSessions = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public AudioRecordingService(
        IOptions<RecordingConfiguration> config,
        ILogger<AudioRecordingService> logger)
    {
        _config = config.Value;
        _logger = logger;

        // Ensure base recording directory exists
        Directory.CreateDirectory(_config.RecordingPath);
    }

    public CallRecordingSession StartRecording(string callId, CallInfo callInfo)
    {
        var recordingId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;
        var fileName = GenerateFileName(callId, startTime);
        var filePath = Path.Combine(_config.RecordingPath, fileName);

        var recordingInfo = new RecordingInfo
        {
            RecordingId = recordingId,
            CallId = callId,
            FileName = fileName,
            FilePath = filePath,
            StartTime = startTime,
            AgentId = callInfo.AgentId,
            AgentName = callInfo.AgentName,
            CallerDisplayName = callInfo.DisplayName,
            PhoneNumber = callInfo.PhoneNumber,
            Direction = callInfo.Direction
        };

        var wavWriter = new WavFileWriter(filePath, 16000, 16, 1);
        var session = new CallRecordingSession(recordingInfo, wavWriter);

        if (_activeSessions.TryAdd(callId, session))
        {
            _logger.LogInformation("Started recording for call {CallId}: {FilePath}", callId, filePath);
            return session;
        }

        // Session already exists, dispose new writer and return existing
        wavWriter.Dispose();
        return _activeSessions[callId];
    }

    public void StopRecording(string callId)
    {
        if (!_activeSessions.TryRemove(callId, out var session))
        {
            _logger.LogWarning("No active recording session found for call {CallId}", callId);
            return;
        }

        try
        {
            // Finalize recording info
            session.RecordingInfo.EndTime = DateTime.UtcNow;
            session.RecordingInfo.FileSizeBytes = session.WavWriter.GetCurrentSize();
            session.RecordingInfo.FinalStatus = CallStatus.Ended;

            // Close WAV file (writes final header)
            session.WavWriter.Dispose();

            // Save metadata JSON alongside WAV
            SaveMetadata(session.RecordingInfo);

            _logger.LogInformation(
                "Stopped recording for call {CallId}. Duration: {Duration}, Size: {Size} bytes",
                callId,
                session.RecordingInfo.Duration,
                session.RecordingInfo.FileSizeBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recording for call {CallId}", callId);
        }
    }

    public RecordingInfo? GetRecordingInfo(string callId)
    {
        return _activeSessions.TryGetValue(callId, out var session)
            ? session.RecordingInfo
            : null;
    }

    private string GenerateFileName(string callId, DateTime startTime)
    {
        // Format: YYYY-MM-DD_HH-mm-ss_callId.wav
        var timestamp = startTime.ToString("yyyy-MM-dd_HH-mm-ss");
        var safeCallId = callId.Length > 8 ? callId[..8] : callId;
        return $"{timestamp}_{safeCallId}.wav";
    }

    private void SaveMetadata(RecordingInfo info)
    {
        var jsonPath = Path.ChangeExtension(info.FilePath, ".json");
        var json = JsonSerializer.Serialize(info, _jsonOptions);
        File.WriteAllText(jsonPath, json);
        _logger.LogInformation("Saved recording metadata: {JsonPath}", jsonPath);
    }

    public void Dispose()
    {
        foreach (var session in _activeSessions.Values)
        {
            try
            {
                session.WavWriter.Dispose();
            }
            catch { }
        }
        _activeSessions.Clear();
    }
}

public class CallRecordingSession
{
    public RecordingInfo RecordingInfo { get; }
    public WavFileWriter WavWriter { get; }
    private readonly object _lock = new();

    public CallRecordingSession(RecordingInfo recordingInfo, WavFileWriter wavWriter)
    {
        RecordingInfo = recordingInfo;
        WavWriter = wavWriter;
    }

    public void WriteAudio(byte[] audioData)
    {
        lock (_lock)
        {
            WavWriter.WriteSamples(audioData);
        }
    }

    public void WriteAudio(byte[] audioData, int offset, int count)
    {
        lock (_lock)
        {
            WavWriter.WriteSamples(audioData, offset, count);
        }
    }
}
