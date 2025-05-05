using System.Text;
using MeetMind.Service.Contracts;
using MeetMind.Service.InteropServices;
using MeetMind.Service.Models;
using Serilog;

namespace MeetMind.Service.Implementations.Androids;

public class WhisperMobileTranscriptionService : ITranscriptionService
{
    public async Task<TranscriptionResult> TranscribeAsync(string audioPath) // Mark method as async
    {
        var result = new TranscriptionResult();
        var buffer = new StringBuilder(10000);
        try
        {
            int code = WhisperInterop.whisper_transcribe(audioPath, "ggml-tiny.bin", buffer, buffer.Capacity);
            if (code == 0)
            {
                result.Success = true;
                result.Transcript = buffer.ToString();
            }
            else
            {
                result.Success = false;
                result.Error = $"Whisper native error code: {code}";
            }
        }
        catch (System.Exception ex)
        {
            Log.Error(ex, "Mobile transcription failed");
            result.Success = false;
            result.Error = ex.Message;
        }
        return await Task.FromResult(result); // Wrap result in a Task
    }
}
