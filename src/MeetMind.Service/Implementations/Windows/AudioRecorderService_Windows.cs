
using MeetMind.Service.Contracts;
using NAudio.Wave;
using Serilog;

namespace MeetMind.Service.Implementations.Windows;

public class AudioRecorderService_Windows : IAudioRecorderService
{
    private WaveInEvent? waveIn;
    private WaveFileWriter? writer;
    public bool IsRecording { get; private set; }

    public async Task StartRecordingAsync(string filePath)
    {
        if (IsRecording)
            throw new InvalidOperationException("Already recording.");

        var dir = Path.GetDirectoryName(filePath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 1)
        };
        writer = new WaveFileWriter(filePath, waveIn.WaveFormat);

        waveIn.DataAvailable += (s, e) =>
        {
            writer.Write(e.Buffer, 0, e.BytesRecorded);
        };
        waveIn.RecordingStopped += (s, e) =>
        {
            writer?.Dispose();
            waveIn.Dispose();
        };

        waveIn.StartRecording();
        IsRecording = true;
        Log.Information("Recording started at {Time}, path: {Path}", DateTime.UtcNow, filePath);
    }

    public async Task StopRecordingAsync()
    {
        if (!IsRecording || waveIn == null)
            throw new InvalidOperationException("No recording in progress.");

        waveIn.StopRecording();
        IsRecording = false;
        Log.Information("Recording stopped at {Time}", DateTime.UtcNow);
    }
}