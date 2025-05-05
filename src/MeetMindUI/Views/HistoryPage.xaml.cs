using MeetMindUI.ViewModels;

namespace MeetMindUI.Views;

public partial class HistoryPage : ContentPage
{
	public HistoryPage(HistoryViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel; // Specify the type argument explicitly
    }
}