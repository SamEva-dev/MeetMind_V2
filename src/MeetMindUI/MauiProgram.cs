using CommunityToolkit.Maui;
using MeetMind.Service.Contracts;

using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Enrichers.WithCaller;
using Serilog.Events;
using MeetMindUI.ViewModels;
using MeetMindUI.Views;
using MeetMind.Service.Implementations;
using MeetMind.Service.Implementations.Windows;



#if ANDROID
using MeetMind.Service.Implementations.Androids;
#elif WINDOWS
using MeetMind.Service.Implementations.Windows;
#endif


namespace MeetMindUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            SetupSerilog();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            CopyCredentialsToAppData().Wait();

            builder.Logging.AddSerilog(dispose: true);
#if ANDROID
            builder.Services.AddSingleton<IAudioRecorderService, AudioRecorderService_Android>()
                            .AddSingleton<ITranscriptionService, WhisperMobileTranscriptionService>();
#elif WINDOWS
            builder.Services.AddSingleton<IAudioRecorderService, AudioRecorderService_Windows>()
                            .AddSingleton<ITranscriptionService, WhisperPythonTranscriptionService>();
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<WhisperPythonService>();
           
            // Register transcription service implementations

            builder.Services.AddSingleton<RecordingViewModel>();
            builder.Services.AddSingleton<RecordingPage>();
            builder.Services.AddTransient<ParticipantsViewModel>();
            builder.Services.AddTransient<SpeechSegmentModalPage>();
            builder.Services.AddSingleton<HistoryPage>();
            builder.Services.AddSingleton<HistoryViewModel>();
            builder.Services.AddSingleton<SettingsPage>();
            builder.Services.AddSingleton<SettingsViewModel>();

            builder.Services.AddSingleton<ISummaryService, SummarizationPythonService>();
            builder.Services.AddSingleton<IVoiceMappingStore, JsonVoiceMappingStore>();
            builder.Services.AddSingleton<ITagGeneratorService, TagGeneratorService>();
            builder.Services.AddSingleton<IGoogleDriveUploaderService, GoogleDriveUploaderService>();
            builder.Services.AddSingleton<ISilenceDetectorService, FfmpegSilenceDetectorService>();
            builder.Services.AddSingleton<ISettingsService, LocalSettingsService>();


            // Enregistrement du service Google Calendar
            builder.Services.AddSingleton<ICalendarService, GoogleCalendarService>(sp =>
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, "credentials.json");
                if (!File.Exists(path))
                {
                    Log.Error("Google credential calendar not found at path: {Path}", path);
                }
                return new GoogleCalendarService(path);
            });


            // Summary service

            return builder.Build();
        }

        private static void SetupSerilog()
        {
            var flushInterval = new TimeSpan(0, 0, 1);
            var file = Path.Combine(FileSystem.AppDataDirectory, "meetmind_log_.log");

            Log.Logger = new LoggerConfiguration()
                .Enrich.WithCaller()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File(file,
                            flushToDiskInterval: flushInterval,
                            encoding: System.Text.Encoding.UTF8,
                            rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: 7,
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({Caller}) {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        }

        private static async Task CopyCredentialsToAppData()
        {
            var targetPath = Path.Combine(FileSystem.AppDataDirectory, "credentials.json");

            // Si déjà copié, rien à faire
            if (File.Exists(targetPath))
                return;

            using var stream = await FileSystem.OpenAppPackageFileAsync("credentials.json");
            using var fs = File.Create(targetPath);
            await stream.CopyToAsync(fs);
        }
    }
}
