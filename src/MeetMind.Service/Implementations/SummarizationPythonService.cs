using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MeetMind.Service.Contracts;
using Serilog;

namespace MeetMind.Service.Implementations;

public class SummarizationPythonService : ISummaryService
{
    public async Task<string> SummarizeAsync(string text)
    {
        var scriptPath = GetPythonScriptPath();
        if (!File.Exists(scriptPath))
        {
            Log.Error("Python script not found at path: {Path}", scriptPath);
            return string.Empty;
        }   

        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{scriptPath}\" \"{text}\"",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(psi);
            await process.StandardInput.WriteAsync(text);
            process.StandardInput.Close();

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(stderr))
            {
                Log.Error("Summary stderr: {Error}", stderr);
            }

            var doc = JsonDocument.Parse(stdout);
            if (doc.RootElement.TryGetProperty("summary", out var node))
                return node.GetString() ?? string.Empty;

            return string.Empty;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to run summarization script");
            return string.Empty;
        }
    }

    string GetPythonScriptPath()
    {
        // Remonte de 5 niveaux depuis le dossier d’exécution pour atteindre la racine du repo
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));
        var scriptPath = Path.Combine(projectRoot, "src", "WhisperPython", "summarize.py");
        return scriptPath;
    }
}