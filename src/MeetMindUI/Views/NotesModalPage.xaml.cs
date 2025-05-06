namespace MeetMindUI.Views;

public partial class NotesModalPage : ContentPage
{

    private readonly string _notesPath;
    private readonly Action? _onSavedCallback;

    public NotesModalPage(string notesPath, Action? onSavedCallback = null)
    {
        InitializeComponent();
        _notesPath = notesPath;
        _onSavedCallback = onSavedCallback;
        LoadNotes();
    }

    private void LoadNotes()
    {
        if (File.Exists(_notesPath))
        {
            NotesEditor.Text = File.ReadAllText(_notesPath);
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            File.WriteAllText(_notesPath, NotesEditor.Text);
            _onSavedCallback?.Invoke();
            await DisplayAlert("Saved", "Notes saved successfully.", "OK");
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Confirm", "Delete the notes for this recording?", "Yes", "Cancel");
        if (!confirm) return;

        try
        {
            if (File.Exists(_notesPath))
            {
                File.Delete(_notesPath);
#if ANDROID || IOS
                try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100)); } catch { }
#endif
            }

            _onSavedCallback?.Invoke();
            await DisplayAlert("Deleted", "Notes deleted.", "OK");
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}