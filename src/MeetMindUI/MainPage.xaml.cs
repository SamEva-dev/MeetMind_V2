using Serilog;

namespace MeetMindUI
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            Log.Information("MeetMind started at {Time}", DateTime.UtcNow);
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";
            Log.Information("MeetMind started at {CounterBtn}", CounterBtn.Text);

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
