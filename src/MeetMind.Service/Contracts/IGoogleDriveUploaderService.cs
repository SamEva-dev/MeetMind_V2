
namespace MeetMind.Service.Contracts;

public interface IGoogleDriveUploaderService
{
    Task UploadRecordingAsync(string zipPath, string meetingName, DateTime date);
}