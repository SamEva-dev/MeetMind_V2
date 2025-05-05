using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetMind.Service.Contracts;
using MeetMindUI.Enums;
using MeetMindUI.Models;

namespace MeetMindUI.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly IVoiceMappingStore _voiceMappingStore;
    private readonly RecordingViewModel _recordingViewModel;

    public ObservableCollection<RecordingItem> Recordings { get; } = new();
    public ObservableCollection<RecordingItem> FilteredRecordings { get; } = new();

    public List<string> SortOptions => Enum.GetNames(typeof(SortOrder)).ToList();


    [ObservableProperty]
    private string searchText;

    [ObservableProperty]
    private DateTime filterDate = DateTime.Today;

    [ObservableProperty]
    private SortOrder selectedSortOrder = SortOrder.NewestFirst;

    public HistoryViewModel(IVoiceMappingStore voiceMappingStore, RecordingViewModel recordingViewModel)
    {
        _voiceMappingStore = voiceMappingStore;
        _recordingViewModel = recordingViewModel;
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
            Recordings.Add(new RecordingItem { FileName = file });
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
        ApplyFilter();
    }


    private void ApplyFilter()
    {
        var filtered = Recordings.Where(r =>
            (string.IsNullOrWhiteSpace(SearchText) || r.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) &&
            r.Created.Date == FilterDate.Date).ToList();

        if (SelectedSortOrder == SortOrder.NewestFirst)
            filtered = filtered.OrderByDescending(r => r.Created).ToList();
        else
            filtered = filtered.OrderBy(r => r.Created).ToList();


        FilteredRecordings.Clear();
        foreach (var r in filtered)
            FilteredRecordings.Add(r);
    }
}
