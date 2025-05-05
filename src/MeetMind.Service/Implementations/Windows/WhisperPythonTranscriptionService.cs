using MeetMind.Service.Contracts;
using MeetMind.Service.Models;

namespace MeetMind.Service.Implementations.Windows;

public class WhisperPythonTranscriptionService : ITranscriptionService
{
    private readonly WhisperPythonService _pythonService;
    public WhisperPythonTranscriptionService(WhisperPythonService pythonService)
    {
        _pythonService = pythonService;
    }

    public async Task<TranscriptionResult> TranscribeAsync(string audioPath)
    {
        return await _pythonService.TranscribeViaPythonAsync(audioPath);
    }
}
