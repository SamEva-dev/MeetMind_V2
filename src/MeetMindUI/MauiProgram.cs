using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Enrichers.WithCaller;
using Serilog.Events;

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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Logging.AddSerilog(dispose: true);

#if DEBUG
            builder.Logging.AddDebug();
#endif

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
    }
}
