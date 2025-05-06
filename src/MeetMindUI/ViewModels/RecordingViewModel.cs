using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using MeetMind.Service.Contracts;
using MeetMindUI.Views;
using Serilog;

namespace MeetMindUI.ViewModels;

public partial class RecordingViewModel : ObservableObject
{
    private readonly IAudioRecorderService _recorderService;
    private readonly ITranscriptionService _transcriptionService;
    private readonly ISummaryService _summaryService;
    private readonly ICalendarService _calendarService;
    private readonly IVoiceMappingStore _voiceMappingStore;
    private readonly ITagGeneratorService _tagGeneratorService;

    [ObservableProperty]
    private bool _isRecording;

    [ObservableProperty]
    private bool _isTranscribing;

    [ObservableProperty]
    private string _statusText = "Ready to record";

    [ObservableProperty]
    private string _transcriptionText = string.Empty;

    [ObservableProperty]
    private string _recordingFilePath = string.Empty;

    [ObservableProperty]
    private bool _isSummarizing;

    [ObservableProperty]
    private string _summaryText = string.Empty;

    public RecordingViewModel(IAudioRecorderService recorderService,
                            ITranscriptionService transcriptionService,
                            ISummaryService summaryService,
                            ICalendarService calendarService,
                            IVoiceMappingStore voiceMappingStore,
                            ITagGeneratorService tagGeneratorService)
    {
        _recorderService = recorderService;
        _transcriptionService = transcriptionService;
        _summaryService = summaryService;
        _calendarService = calendarService;
        _voiceMappingStore = voiceMappingStore;
        _voiceMappingStore = voiceMappingStore;
        _tagGeneratorService = tagGeneratorService;
    }

    [RelayCommand(CanExecute = nameof(CanStartRecording))]
    private async Task StartRecordingAsync()
    {
        // 1. Récupérer les participants depuis Google Calendar
        /*var start = DateTime.Now;
        var end = start.AddHours(1);
        var participants = await _calendarService.GetParticipantsAsync(start, end);

        // 2. Affichage simple via ActionSheet pour sélectionner les présents
        if (participants.Count > 0)
        {
            var selected = await Application.Current.MainPage.DisplayActionSheet(
                "Who is present?", "Cancel", null, participants.ToArray());

            if (selected == "Cancel" || string.IsNullOrEmpty(selected))
            {
                StatusText = "Recording cancelled";
                return;
            }

            Log.Information("Selected participant: {Participant}", selected);
        }
        */

        /*
          var now = DateTime.Now;
            var calendarService = Ioc.Default.GetService<ICalendarService>();
            var vm = new ParticipantsViewModel(calendarService, now, now.AddHours(1));
            var modal = new ParticipantsModal(vm);

            await Application.Current.MainPage.Navigation.PushModalAsync(modal);
         */

        // Demande de la permission microphone
        var micStatus = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (micStatus != PermissionStatus.Granted)
            micStatus = await Permissions.RequestAsync<Permissions.Microphone>();
        if (micStatus != PermissionStatus.Granted)
        {
            StatusText = "Microphone permission denied";
            return;
        }

        try
        {
            var filename = $"recording_{DateTime.UtcNow:yyyyMMdd_HHmmss}.3gp";
            var filepath = Path.Combine(FileSystem.AppDataDirectory, filename);
            await PrepareDirectoryAsync(filepath);
            await _recorderService.StartRecordingAsync(filepath);

            RecordingFilePath = filepath;
            IsRecording = true;
            StatusText = "Recording...";
            Log.Information("Recording started: {Path}", filepath);
        }
        catch (Exception ex)
        {
            StatusText = "Error starting recording";
            Log.Error(ex, "Failed to start recording");
        }
        StartRecordingCommand.NotifyCanExecuteChanged();
        StopRecordingCommand.NotifyCanExecuteChanged();
    }

    private bool CanStartRecording() => !IsRecording;

    [RelayCommand(CanExecute = nameof(CanStopRecording))]
    private async Task StopRecordingAsync()
    {
        try
        {
            await _recorderService.StopRecordingAsync();
            IsRecording = false;
            StatusText = "Recording stopped";
            Log.Information("Recording stopped");

            // Trigger transcription
            await PerformTranscriptionAndSummaryAsync();
        }
        catch (Exception ex)
        {
            StatusText = "Error during transcription";
            Log.Error(ex, "Transcription process failed");
        }
        finally
        {
            IsTranscribing = false;
            StatusText = "Ready to record";
        }

        StartRecordingCommand.NotifyCanExecuteChanged();
        StopRecordingCommand.NotifyCanExecuteChanged();
    }
    [RelayCommand]
    private async Task GenerateSummaryAsync()
    {
        if (string.IsNullOrWhiteSpace(TranscriptionText))
            return;

        IsSummarizing = true;
        StatusText = "Summarizing...";

        SummaryText = await _summaryService.SummarizeAsync(TranscriptionText);

        // save summary file
        var summaryPath = Path.ChangeExtension(RecordingFilePath, ".summary.txt");
        File.WriteAllText(summaryPath, SummaryText, Encoding.UTF8);
        Log.Information("Summary saved: {Path}", summaryPath);

        IsSummarizing = false;
        StatusText = "Ready to record";
    }

    [RelayCommand]
    public async Task ReassignSpeakersAsync()
    {
        if (string.IsNullOrWhiteSpace(TranscriptionText))
        {
            await Application.Current.MainPage.DisplayAlert("Info", "No transcription loaded.", "OK");
            return;
        }

        var speakerIds = new Regex(@"\[(SPEAKER_\d{2})\]")
            .Matches(TranscriptionText)
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .ToList();

        foreach (var speakerId in speakerIds)
        {
            var existing = await _voiceMappingStore.GetMappedNameAsync(speakerId);

            var prompt = await Application.Current.MainPage.DisplayPromptAsync(
                "Reassign Speaker", $"Who is [{speakerId}]?", "Save", "Skip",
                existing ?? "Ex: Claire", -1, Keyboard.Text);

            if (!string.IsNullOrWhiteSpace(prompt))
            {
                await _voiceMappingStore.SaveMappingAsync(speakerId, prompt);
                Log.Information("Reassigned {SpeakerId} to {Name}", speakerId, prompt);
            }
        }

        // Re-apply updated mappings
        TranscriptionText = await ReplaceSpeakerIdsAsync(TranscriptionText);
        OnPropertyChanged(nameof(TranscriptionText));

        // Save new version
        var updatedPath = Path.ChangeExtension(RecordingFilePath, ".txt");
        File.WriteAllText(updatedPath, TranscriptionText);
        Log.Information("Updated transcription saved: {Path}", updatedPath);
    }

    private bool CanStopRecording() => IsRecording;

    private async Task PerformTranscriptionAndSummaryAsync()
    {
        try
        {
            IsTranscribing = true;
            StatusText = "Transcribing...";

            var transcriptionResult = await _transcriptionService.TranscribeAsync(RecordingFilePath);
            TranscriptionText = transcriptionResult.Success ? transcriptionResult.Transcript ?? string.Empty : string.Empty;
            OnPropertyChanged(nameof(TranscriptionText));

            // Remplacer les SPEAKER_ID par les noms connus
            TranscriptionText = await ReplaceSpeakerIdsAsync(TranscriptionText);
            OnPropertyChanged(nameof(TranscriptionText));

            // Sauvegarde initiale
            var txtPath = Path.ChangeExtension(RecordingFilePath, ".txt");
            File.WriteAllText(txtPath, TranscriptionText, Encoding.UTF8);
            Log.Information("Transcription saved: {Path}", txtPath);

            // Association manuelle des speakers inconnus
            var unknownSpeakers = await GetUnknownSpeakerIdsAsync(TranscriptionText);
            foreach (var speakerId in unknownSpeakers)
            {
                var name = await Application.Current.MainPage.DisplayPromptAsync(
                    "Assign a Name", $"Who is [{speakerId}] ?", "Save", "Cancel", "Ex: Luc", -1, Keyboard.Text);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    await _voiceMappingStore.SaveMappingAsync(speakerId, name);
                    Log.Information("Assigned {SpeakerId} to {Name}", speakerId, name);
                }
            }

            // Remplacer à nouveau les noms
            TranscriptionText = await ReplaceSpeakerIdsAsync(TranscriptionText);
            OnPropertyChanged(nameof(TranscriptionText));

            // Resave mise à jour
            File.WriteAllText(txtPath, TranscriptionText, Encoding.UTF8);
            Log.Information("Updated transcription saved: {Path}", txtPath);
        }
        catch (Exception ex)
        {
            StatusText = "Error during transcription";
            Log.Error(ex, "Transcription failed");
        }
        finally
        {
            IsTranscribing = false;
        }

        // Trigger summary automatically
        try
        {
            IsSummarizing = true;
            StatusText = "Summarizing...";

            SummaryText = await _summaryService.SummarizeAsync(TranscriptionText);
            OnPropertyChanged(nameof(SummaryText));

            // Save summary
            var summaryPath = Path.ChangeExtension(RecordingFilePath, ".summary.txt");
            File.WriteAllText(summaryPath, SummaryText, Encoding.UTF8);
            Log.Information("Summary saved: {Path}", summaryPath);
        }
        catch (Exception ex)
        {
            StatusText = "Error during summarization";
            Log.Error(ex, "Summarization failed");
        }
        finally
        {
            IsSummarizing = false;
            StatusText = "Ready to record";
        }
    }

    public async Task ProcessAfterTranscriptionAsync()
    {
        var transcriptPath = Path.ChangeExtension(RecordingFilePath, ".txt");
        File.WriteAllText(transcriptPath, TranscriptionText);

        var summary = await _summaryService.SummarizeAsync(TranscriptionText);
        var summaryPath = Path.ChangeExtension(RecordingFilePath, ".summary.txt");
        File.WriteAllText(summaryPath, summary);

        var tags = await _tagGeneratorService.GenerateTagsAsync(TranscriptionText);
        var tagPath = Path.ChangeExtension(RecordingFilePath, ".tags.json");
        File.WriteAllText(tagPath, JsonSerializer.Serialize(tags));
    }

    private async Task<string> ReplaceSpeakerIdsAsync(string rawText)
    {
        var speakerPattern = new Regex(@"\[(SPEAKER_\d{2})\]");
        var allMappings = await _voiceMappingStore.GetAllAsync();

        return speakerPattern.Replace(rawText, match =>
        {
            var id = match.Groups[1].Value;
            var known = allMappings.FirstOrDefault(x => x.Id == id);
            return known != null ? $"[{known.Name}]" : match.Value;
        });
    }

    private async Task<List<string>> GetUnknownSpeakerIdsAsync(string text)
    {
        var regex = new Regex(@"\[(SPEAKER_\d{2})\]");
        var matches = regex.Matches(text).Select(m => m.Groups[1].Value).Distinct();
        var known = (await _voiceMappingStore.GetAllAsync()).Select(x => x.Id);
        return matches.Except(known).ToList();
    }


    private Task PrepareDirectoryAsync(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir!);
        }
        return Task.CompletedTask;
    }
}