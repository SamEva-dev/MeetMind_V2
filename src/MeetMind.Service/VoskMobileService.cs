using Vosk;
using Microsoft.Maui.Storage;

namespace MeetMind.Service;

public class VoskMobileService
{
    private Model? _model;

    // Charge le modèle depuis AppDataDirectory
    public void EnsureModelLoaded(string modelDir)
    {
        if (_model != null) return;
        _model = new Model(modelDir);
    }

    // Transcrit un fichier WAV (mono, 16 kHz PCM) en texte
    public string Transcribe(string audioPath)
    {
        if (_model == null)
            throw new InvalidOperationException("Model not loaded");

        // Création du recognizer
        using var rec = new VoskRecognizer(_model, 16000.0f);
        using var fs = new FileStream(audioPath, FileMode.Open, FileAccess.Read);

        // Skip 44-byte WAV header
        fs.Position = 44;
        byte[] buffer = new byte[4096];
        int bytesRead;
        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
        {
            rec.AcceptWaveform(buffer, bytesRead);
        }

        // Récupère le JSON final et retourne le champ "text"
        var resultJson = rec.FinalResult();
        using var doc = System.Text.Json.JsonDocument.Parse(resultJson);
        return doc.RootElement.GetProperty("text").GetString() ?? "";
    }
}