using MeetMind.Service.Models;

namespace MeetMind.Service.Contracts;

public interface ITranscriptionService
{
    Task<TranscriptionResult> TranscribeAsync(string audioPath);
}
