

namespace MeetMind.Service.Contracts;

public interface IAudioRecorderService
{
    Task StartRecordingAsync(string filePath);
    Task StopRecordingAsync();
    bool IsRecording { get; }
}