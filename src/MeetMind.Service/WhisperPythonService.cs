using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MeetMind.Service.Models;

namespace MeetMind.Service;

public class WhisperPythonService
{
    public  async Task<TranscriptionResult> TranscribeViaPythonAsync(string audioPath)
    {
#if WINDOWS
    var result = new TranscriptionResult();

    try
    {
        var scriptPath = GetPythonScriptPath();

        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{scriptPath}\" \"{audioPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };


        using var process = new Process { StartInfo = psi };
        process.Start();

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        result.Log = stderr.Trim();

        if (!string.IsNullOrWhiteSpace(stderr) && stderr.Contains("[ERROR]"))
        {
            result.Success = false;
            result.Error = stderr;
            return result;
        }

        result.Success = true;
        using var doc = JsonDocument.Parse(stdout);
        if (doc.RootElement.TryGetProperty("text", out var output ))
            result.Transcript = output.GetString();
        else
           result.Transcript = "[Empty transcript] or [No text field found]";
        return result;
    }
    catch (Exception ex)
    {
        return new TranscriptionResult
        {
            Success = false,
            Error = ex.Message
        };
    }
#else
        return new TranscriptionResult
        {
            Success = false,
            Error = "Transcription Python uniquement disponible sur Windows."
        };
#endif
    }


    string GetPythonScriptPath()
    {
        // Remonte de 5 niveaux depuis le dossier d’exécution pour atteindre la racine du repo
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));
        var scriptPath = Path.Combine(projectRoot, "src", "WhisperPython", "transcribe.py");
        return scriptPath;
    }
}