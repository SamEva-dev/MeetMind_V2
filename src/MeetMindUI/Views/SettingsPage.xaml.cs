using MeetMindUI.ViewModels;

namespace MeetMindUI.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel; // Specify the type argument explicitly
    }
}