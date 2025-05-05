using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MeetMind.Service;
using MeetMind.Service.Contracts;


namespace MeetMind.Service.Implementations;

public class GoogleCalendarService : ICalendarService
{
    private const string ApplicationName = "MeetMind";
    private readonly string[] Scopes = { CalendarService.Scope.CalendarReadonly };
    private readonly string _credentialsFilePath;
    private CalendarService _calendarService;
    public GoogleCalendarService(string credentialsFilePath)
    {
        _credentialsFilePath = credentialsFilePath;
        Initialize().Wait();
    }

    private async Task Initialize()
    {
        UserCredential credential;
        using (var stream = await FileSystem.OpenAppPackageFileAsync("credentials.json"))
        {
            // The file token.json stores the user's access and refresh tokens.
            string tokenPath = Path.Combine(FileSystem.AppDataDirectory, "token.json");
            var secret = GoogleClientSecrets.FromStream(stream).Secrets;
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secret,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(tokenPath, true));
        }

        _calendarService = new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }

    public async Task<IList<string>> GetParticipantsAsync(DateTime start, DateTime end)
    {
        var request = _calendarService.Events.List("primary");
        request.TimeMin = start;
        request.TimeMax = end;
        request.ShowDeleted = false;
        request.SingleEvents = true;

        Events events = await request.ExecuteAsync();
        var participants = new List<string>();

        foreach (var ev in events.Items)
        {
            if (ev.Attendees != null)
            {
                foreach (var attendee in ev.Attendees)
                {
                    if (attendee.ResponseStatus == "accepted" || attendee.ResponseStatus == "tentative")
                    {
                        participants.Add(attendee.DisplayName ?? attendee.Email);
                    }
                }
            }
        }

        return participants;
    }
}