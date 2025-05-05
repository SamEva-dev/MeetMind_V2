using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeetMind.Service.Contracts;
using MeetMind.Service.Models;
using Serilog;

namespace MeetMindUI.ViewModels;

public partial class ParticipantsViewModel : ObservableObject
{
    private readonly ICalendarService _calendarService;
    private readonly DateTime _meetingStart;
    private readonly DateTime _meetingEnd;
    public ObservableCollection<ParticipantItem> Participants { get; } = new();

    public ParticipantsViewModel(ICalendarService calendarService, DateTime meetingStart, DateTime meetingEnd)
    {
        _calendarService = calendarService;
        _meetingStart = meetingStart;
        _meetingEnd = meetingEnd;
        LoadParticipantsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadParticipantsAsync()
    {
        try
        {
            var names = await _calendarService.GetParticipantsAsync(_meetingStart, _meetingEnd);
            Participants.Clear();
            foreach (var name in names)
                Participants.Add(new ParticipantItem { Name = name, IsPresent = true });
            StatusText = "Select participants present:";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load calendar participants");
            StatusText = "Error loading participants.";
        }
    }

    [ObservableProperty]
    private string statusText;

    [RelayCommand]
    private async Task ConfirmSelectionAsync()
    {
        // On confirmation, pop modal and return selected items via MessagingCenter or callback
        // For now, simply log selections
        var selected = Participants.Where(p => p.IsPresent).Select(p => p.Name);
        Log.Information("Participants confirmed: {Names}", string.Join(", ", selected));
        // Close modal
        await App.Current.MainPage.Navigation.PopModalAsync();
    }
}