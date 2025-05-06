using System.Collections.ObjectModel;
using MeetMind.Service.Contracts;

namespace MeetMindUI.Views;

public partial class SpeechSegmentModalPage : ContentPage
{
    public ObservableCollection<string> Segments { get; } = new();
    public SpeechSegmentModalPage(List<SpeechSegment> detectedSegments)
	{
		InitializeComponent();

        foreach (var seg in detectedSegments)
            Segments.Add($@"{seg.Start:hh\:mm\:ss} → {seg.End:hh\:mm\:ss}");

        BindingContext = this;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}