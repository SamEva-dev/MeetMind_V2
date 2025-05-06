using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetMind.Service.Contracts;
using MeetMind.Service.Models;

namespace MeetMindUI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private UserSettings settings = new();

    public List<string> AvailableLanguages { get; } = new() { "auto", "en", "fr", "es", "de", "pt" };

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        LoadSettings();
    }

    private async void LoadSettings()
    {
        Settings = await _settingsService.LoadAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await _settingsService.SaveAsync(Settings);
        await Shell.Current.DisplayAlert("Saved", "Settings saved successfully.", "OK");
    }
}