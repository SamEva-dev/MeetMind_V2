#if ANDROID
using Android.Media;
using Serilog;
using MeetMind.Service.Contracts;

namespace MeetMind.Service.Implementations.Androids;

public class AudioRecorderService_Android : IAudioRecorderService
{

    MediaRecorder? _recorder;
    string? _currentFilePath;

    public bool IsRecording { get; private set; }

    public Task StartRecordingAsync(string filePath)
    {
        if (IsRecording)
            throw new InvalidOperationException("Already recording.");

        _currentFilePath = filePath;

        // Crée le dossier si nécessaire
        var dir = Path.GetDirectoryName(filePath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        _recorder = new MediaRecorder();
        _recorder.SetAudioSource(AudioSource.Mic);
        _recorder.SetOutputFormat(OutputFormat.ThreeGpp);
        _recorder.SetAudioEncoder(AudioEncoder.AmrNb);
        _recorder.SetOutputFile(filePath);

        try
        {
            _recorder.Prepare();
            _recorder.Start();
            IsRecording = true;
            Log.Information("Recording started at {Time}, path: {Path}", DateTime.UtcNow, filePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start recording");
            throw;
        }

        return Task.CompletedTask;
    }

    public Task StopRecordingAsync()
    {
        if (!IsRecording || _recorder == null)
            throw new InvalidOperationException("No recording in progress.");

        try
        {
            _recorder.Stop();
            _recorder.Release();
            Log.Information("Recording stopped at {Time}, path: {Path}", DateTime.UtcNow, _currentFilePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to stop recording");
            throw;
        }
        finally
        {
            _recorder = null;
            IsRecording = false;
        }

        return Task.CompletedTask;
    }
}
#endif