using System.IO.Compression;
using System.Text;
using MeetMind.Service;
using MeetMindUI.InteropServices;
using Serilog;

namespace MeetMindUI
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            Log.Information("MeetMind started at {Time}", DateTime.UtcNow);
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";
            Log.Information("MeetMind started at {CounterBtn}", CounterBtn.Text);

            SemanticScreenReader.Announce(CounterBtn.Text);

        }

        private async void OnStartClicked(object sender, EventArgs e)
        {
            try
            {
                // Préparer les chemins dans le répertoire local
                var audioPath = Path.Combine(FileSystem.AppDataDirectory, "test1.wav");
                var modelPath = Path.Combine(FileSystem.AppDataDirectory, "ggml-tiny.bin");

                // Copier test.wav depuis les assets vers le répertoire local
                if (!File.Exists(audioPath))
                {
                    using var audioStream = await FileSystem.OpenAppPackageFileAsync("test.wav");
                    using var fileStream = File.Create(audioPath);
                    await audioStream.CopyToAsync(fileStream);
                }

                // Copier ggml-tiny.bin depuis les assets vers le répertoire local
                if (!File.Exists(modelPath))
                {
                    using var modelStream = await FileSystem.OpenAppPackageFileAsync("ggml-tiny.bin");
                    using var fileStream = File.Create(modelPath);
                    await modelStream.CopyToAsync(fileStream);
                }

                // Créer un buffer pour la sortie texte
                var outputBuffer = new StringBuilder(8192); // 8 Ko
#if ANDROID
                try
                {

                    Java.Lang.JavaSystem.LoadLibrary("whisper");

                Console.WriteLine("libwhisper.so loaded");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur de chargement : " + ex.Message);
                }

                // Appeler la fonction native (interop avec whisper.cpp)
                int resultCode = WhisperInterop.whisper_transcribe(audioPath, modelPath, outputBuffer, outputBuffer.Capacity);

                if (resultCode == 0)
                {
                    string transcription = outputBuffer.ToString();
                    await DisplayAlert("Transcription", transcription, "OK");
                }
                else
                {
                    await DisplayAlert("Erreur", $"Échec de la transcription. Code retour : {resultCode}", "Fermer");
                }
#endif
#if WINDOWS || MACCATALYST
                var service = new WhisperPythonService();
                var result = await service.TranscribeViaPythonAsync(audioPath);
                await DisplayAlert("Desktop Transcription", result.Transcript, "OK");
#else
                await DisplayAlert("Non-desktop platform", "This test runs only on desktop.", "Close");
#endif
            }
            catch (Exception ex)
            {
                await DisplayAlert("Exception", ex.Message, "Fermer");
            }
        }


        public async Task<string> TranscribeAudioMobile(string audioPath, string modelPath)
        {
            StringBuilder result = new StringBuilder(10000);
            int status = WhisperInterop.whisper_transcribe(audioPath, modelPath, result, result.Capacity);

            if (status == 0)
                return result.ToString();
            else
                return "[ERROR] Whisper transcription failed.";
        }

        private async Task PrepareVoskAsync()
        {
            string zipName = "vosk-model-small-fr.zip";
            string zipDest = Path.Combine(FileSystem.AppDataDirectory, zipName);
            string modelDir = Path.Combine(FileSystem.AppDataDirectory, "vosk-model-small-fr");

            // Copier le zip si pas déjà présent
            if (!File.Exists(zipDest))
            {
                using var src = await FileSystem.OpenAppPackageFileAsync(zipName);
                using var dst = File.Create(zipDest);
                await src.CopyToAsync(dst);
            }

            // Décompresser si nécessaire
            if (!Directory.Exists(modelDir))
            {
                ZipFile.ExtractToDirectory(zipDest, modelDir);
            }
        }

        private async void OnVoskTranscribeClicked(object sender, EventArgs e)
        {
            await PrepareVoskAsync();

            string modelDir = Path.Combine(FileSystem.AppDataDirectory, "vosk-model-small-fr");
            string audioPath = Path.Combine(FileSystem.AppDataDirectory, "test.wav"); // déjà copié via ton code

            var service = new VoskMobileService();
            service.EnsureModelLoaded(modelDir);

            string transcript = service.Transcribe(audioPath);

            await DisplayAlert("Vosk Transcription", transcript, "OK");
            Log.Information("Vosk transcribed text: {Text}", transcript);
        }

        private static readonly string[] ModelFiles = new[]
{
    "model/am",
    "model/conf/config.json",
    "model/ivector.scp",
    // etc.
};

        private async Task CopyModelFilesAsync()
        {
            string modelName = "vosk-model-small-fr";
            string destDir = Path.Combine(FileSystem.AppDataDirectory, modelName);

            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            foreach (var relativePath in ModelFiles)
            {
                var assetPath = Path.Combine(modelName, relativePath);
                using var src = await FileSystem.OpenAppPackageFileAsync(assetPath);
                var destPath = Path.Combine(destDir, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                using var dst = File.Create(destPath);
                await src.CopyToAsync(dst);
            }
        }

    }

}
