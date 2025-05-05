using CommunityToolkit.Mvvm.DependencyInjection;
using MeetMindUI.ViewModels;
using Microsoft.Maui.Controls;
namespace MeetMindUI.Views;

public partial class RecordingPage : ContentPage
{
    public RecordingPage(RecordingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel; // Specify the type argument explicitly
    }
}
