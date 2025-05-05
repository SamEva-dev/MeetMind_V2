using CommunityToolkit.Mvvm.DependencyInjection;
using MeetMind.Service.Contracts;
using MeetMindUI.ViewModels;

namespace MeetMindUI.Views;

public partial class ParticipantsModal : ContentPage
{
    public ParticipantsModal(ParticipantsViewModel vm)
    {
        InitializeComponent();
        //var calendarService = Ioc.Default.GetService<ICalendarService>();

        BindingContext = vm;
    }
}
