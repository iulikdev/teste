namespace TeamsCallCenter.Api.Bot.Audio;

/// <summary>
/// Writes PCM audio data to WAV file format.
/// WAV format: 44-byte header + PCM data
/// </summary>
public class WavFileWriter : IDisposable
{
    private readonly FileStream _fileStream;
    private readonly BinaryWriter _writer;
    private readonly int _sampleRate;
    private readonly int _bitsPerSample;
    private readonly int _channels;
    private long _dataSize;
    private bool _disposed;
    private bool _headerWritten;

    public string FilePath { get; }

    public WavFileWriter(string filePath, int sampleRate = 16000, int bitsPerSample = 16, int channels = 1)
    {
        FilePath = filePath;
        _sampleRate = sampleRate;
        _bitsPerSample = bitsPerSample;
        _channels = channels;
        _dataSize = 0;

        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        _writer = new BinaryWriter(_fileStream);

        WriteWavHeader();
        _headerWritten = true;
    }

    private void WriteWavHeader()
    {
        int byteRate = _sampleRate * _channels * _bitsPerSample / 8;
        short blockAlign = (short)(_channels * _bitsPerSample / 8);

        // RIFF header
        _writer.Write("RIFF"u8);
        _writer.Write(0); // Placeholder for file size - 8, updated on close
        _writer.Write("WAVE"u8);

        // fmt subchunk
        _writer.Write("fmt "u8);
        _writer.Write(16); // Subchunk1Size (16 for PCM)
        _writer.Write((short)1); // AudioFormat (1 = PCM)
        _writer.Write((short)_channels);
        _writer.Write(_sampleRate);
        _writer.Write(byteRate);
        _writer.Write(blockAlign);
        _writer.Write((short)_bitsPerSample);

        // data subchunk
        _writer.Write("data"u8);
        _writer.Write(0); // Placeholder for data size, updated on close
    }

    public void WriteSamples(byte[] audioData)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(WavFileWriter));

        _writer.Write(audioData);
        _dataSize += audioData.Length;
    }

    public void WriteSamples(byte[] audioData, int offset, int count)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(WavFileWriter));

        _writer.Write(audioData, offset, count);
        _dataSize += count;
    }

    public long GetCurrentSize() => _dataSize;

    private void FinalizeWavHeader()
    {
        if (!_headerWritten) return;

        _writer.Flush();

        // Update RIFF chunk size (file size - 8)
        _fileStream.Seek(4, SeekOrigin.Begin);
        _writer.Write((int)(_dataSize + 36));

        // Update data chunk size
        _fileStream.Seek(40, SeekOrigin.Begin);
        _writer.Write((int)_dataSize);

        _fileStream.Seek(0, SeekOrigin.End);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            FinalizeWavHeader();
        }
        finally
        {
            _writer.Dispose();
            _fileStream.Dispose();
        }
    }
}
