using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetMind.Service.Contracts;
using Serilog;

namespace MeetMindUI.ViewModels;

public partial class RecordingViewModel : ObservableObject
{
    private readonly IAudioRecorderService _recorderService;

    [ObservableProperty]
    private bool _isRecording;

    [ObservableProperty]
    private string _statusText = "Ready to record";

    public RecordingViewModel(IAudioRecorderService recorderService)
    {
        _recorderService = recorderService;
    }

    [RelayCommand(CanExecute = nameof(CanStartRecording))]
    private async Task StartRecordingAsync()
    {
        // Demande de la permission microphone
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Microphone>();
        }
        if (status != PermissionStatus.Granted)
        {
            StatusText = "Microphone permission denied";
            return;
        }

        try
        {
            var fileName = $"recording_{DateTime.UtcNow:yyyyMMdd_HHmmss}.3gp";
            var filePath = System.IO.Path.Combine(FileSystem.AppDataDirectory, fileName);

            await PrepareDirectoryAsync(filePath);
            await _recorderService.StartRecordingAsync(filePath);

            IsRecording = true;
            StatusText = "Recording...";
            Log.Information("Recording started: {Path}", filePath);
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
        }
        catch (Exception ex)
        {
            StatusText = "Error stopping recording";
            Log.Error(ex, "Failed to stop recording");
        }
        StartRecordingCommand.NotifyCanExecuteChanged();
        StopRecordingCommand.NotifyCanExecuteChanged();
    }

    private bool CanStopRecording() => IsRecording;

    private Task PrepareDirectoryAsync(string filePath)
    {
        var dir = System.IO.Path.GetDirectoryName(filePath);
        if (!System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir!);
        }
        return Task.CompletedTask;
    }
}