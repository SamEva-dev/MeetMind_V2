using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetMind.Service.Contracts;
using Serilog;

namespace MeetMindUI.ViewModels;

public partial class RecordingViewModel : ObservableObject
{
    private readonly IAudioRecorderService _recorderService;
    private readonly ITranscriptionService _transcriptionService;
    private readonly ISummaryService _summaryService;

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
                            ISummaryService summaryService)
    {
        _recorderService = recorderService;
        _transcriptionService = transcriptionService;
        _summaryService = summaryService;
    }

    [RelayCommand(CanExecute = nameof(CanStartRecording))]
    private async Task StartRecordingAsync()
    {
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

            // Save transcription
            var txtPath = Path.ChangeExtension(RecordingFilePath, ".txt");
            File.WriteAllText(txtPath, TranscriptionText, Encoding.UTF8);
            Log.Information("Transcription saved: {Path}", txtPath);
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