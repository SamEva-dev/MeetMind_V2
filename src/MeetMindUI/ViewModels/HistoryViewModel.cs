using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetMind.Service.Contracts;
using MeetMindUI.Enums;
using MeetMindUI.Models;
using MeetMindUI.Views;

namespace MeetMindUI.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly IVoiceMappingStore _voiceMappingStore;
    private readonly RecordingViewModel _recordingViewModel;
    private readonly IGoogleDriveUploaderService _driveUploaderService;

    public ObservableCollection<RecordingItem> Recordings { get; } = new();
    public ObservableCollection<RecordingItem> FilteredRecordings { get; } = new();

    public List<string> SortOptions => Enum.GetNames(typeof(SortOrder)).ToList();

    private readonly Dictionary<string, string> _lastExports = new();



    [ObservableProperty]
    private string searchText;

    [ObservableProperty]
    private DateTime filterDate = DateTime.Today;

    [ObservableProperty]
    private int selectedSortOrder = 0;

    public HistoryViewModel(IVoiceMappingStore voiceMappingStore, 
        RecordingViewModel recordingViewModel,
        IGoogleDriveUploaderService driveUploaderService)
    {
        _voiceMappingStore = voiceMappingStore;
        _recordingViewModel = recordingViewModel;
        _driveUploaderService = driveUploaderService;
        Load();
    }

    [RelayCommand]
    public void Load()
    {
        Recordings.Clear();
        var dir = FileSystem.AppDataDirectory;
        var files = Directory.GetFiles(dir, "*.3gp");
        foreach (var file in files)
        {
            var recording = new RecordingItem { FileName = file };
            if (File.Exists(recording.NotesPath) && new FileInfo(recording.NotesPath).Length > 0)
            {
                recording.HasNotes = true;
            }
            Recordings.Add(recording);
        }

        ApplyFilter();
    }

    [RelayCommand]
    private async Task PlayAsync(RecordingItem item)
    {
        await Application.Current.MainPage.DisplayAlert("Play", $"Would play {item.AudioPath}", "OK");
        // TODO: Implémenter un vrai lecteur audio si besoin
    }

    [RelayCommand]
    private async Task ReassignAsync(RecordingItem item)
    {
        _recordingViewModel.RecordingFilePath = item.AudioPath;
        _recordingViewModel.TranscriptionText = File.ReadAllText(item.TranscriptPath);
        await _recordingViewModel.ReassignSpeakersAsync();
    }

    [RelayCommand]
    private async Task DeleteAsync(RecordingItem item)
    {
        if (File.Exists(item.AudioPath)) File.Delete(item.AudioPath);
        if (File.Exists(item.TranscriptPath)) File.Delete(item.TranscriptPath);
        if (File.Exists(item.SummaryPath)) File.Delete(item.SummaryPath);
        Recordings.Remove(item);
        ApplyFilter();
    }

    [RelayCommand]
    private async Task FilterAsync()
    {
        ApplyFilter();
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        SearchText = string.Empty;
        FilterDate = DateTime.Today;
        SelectedSortOrder = 0;
        ApplyFilter();
    }

    [RelayCommand]
    private async Task ExportAsync(RecordingItem item)
    {
        try
        {
            var tempDir = Path.Combine(FileSystem.CacheDirectory, "Export_" + Path.GetFileNameWithoutExtension(item.FileName));
            Directory.CreateDirectory(tempDir);

            var filesToExport = new List<string>();

            // Copy .3gp
            if (File.Exists(item.AudioPath))
            {
                var target = Path.Combine(tempDir, Path.GetFileName(item.AudioPath));
                File.Copy(item.AudioPath, target, true);
                filesToExport.Add(target);
            }

            // Copy .txt
            if (File.Exists(item.TranscriptPath))
            {
                var target = Path.Combine(tempDir, Path.GetFileName(item.TranscriptPath));
                File.Copy(item.TranscriptPath, target, true);
                filesToExport.Add(target);
            }

            // Copy .summary.txt
            if (File.Exists(item.SummaryPath))
            {
                var target = Path.Combine(tempDir, Path.GetFileName(item.SummaryPath));
                File.Copy(item.SummaryPath, target, true);
                filesToExport.Add(target);
            }

            var zipPath = Path.Combine(FileSystem.AppDataDirectory, $"{item.DisplayName}_export.zip");

            if (File.Exists(zipPath))
                File.Delete(zipPath);

            System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, zipPath);

            await Application.Current.MainPage.DisplayAlert("Export", $"Exported to: {zipPath}", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task ShareAsync(RecordingItem item)
    {
        if (!_lastExports.TryGetValue(item.FileName, out var zipPath) || !File.Exists(zipPath))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No exported file found. Export first.", "OK");
            return;
        }

        await Share.RequestAsync(new ShareFileRequest
        {
            Title = $"Share {item.DisplayName}",
            File = new ShareFile(zipPath)
        });
    }

    [RelayCommand]
    private async Task TagClickAsync(string tag)
    {
        SearchText = tag;
        ApplyFilter();
    }

    [RelayCommand]
    private async Task UploadToDriveAsync(RecordingItem item)
    {
        if (!_lastExports.TryGetValue(item.FileName, out var zipPath) || !File.Exists(zipPath))
        {
            await Application.Current.MainPage.DisplayAlert("Info", "Exporting file before upload...", "OK");
            await ExportAsync(item);
            zipPath = _lastExports[item.FileName];
        }

        await _driveUploaderService.UploadRecordingAsync(zipPath, item.DisplayName, item.Created);
        await Application.Current.MainPage.DisplayAlert("Google Drive", "Upload completed successfully.", "OK");
    }

    [RelayCommand]
    private async Task EditNotesAsync(RecordingItem item)
    {
        var page = new NotesModalPage(item.NotesPath, () =>
        {
            item.HasNotes = File.Exists(item.NotesPath) && new FileInfo(item.NotesPath).Length > 0;
        });
        await Application.Current.MainPage.Navigation.PushModalAsync(page);
    }

    private void ApplyFilter()
    {
        var filtered = Recordings.Where(r =>
            (string.IsNullOrWhiteSpace(SearchText) || r.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) &&
            r.Created.Date == FilterDate.Date).ToList();

        if ((SortOrder)SelectedSortOrder == SortOrder.NewestFirst)
            filtered = filtered.OrderByDescending(r => r.Created).ToList();
        else
            filtered = filtered.OrderBy(r => r.Created).ToList();


        FilteredRecordings.Clear();
        foreach (var r in filtered)
            FilteredRecordings.Add(r);
    }
}
