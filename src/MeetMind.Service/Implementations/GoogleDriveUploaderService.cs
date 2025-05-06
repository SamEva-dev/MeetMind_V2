using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MeetMind.Service.Contracts;

namespace MeetMind.Service.Implementations;

public class GoogleDriveUploaderService : IGoogleDriveUploaderService
{
    private const string ApplicationName = "MeetMindUploader";
    private const string MeetMindFolderName = "MeetMind";

    public async Task UploadRecordingAsync(string zipPath, string meetingName, DateTime date)
    {
        var credentialPath = Path.Combine(FileSystem.AppDataDirectory, "credentials.json");

        using var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read);
        var credPath = Path.Combine(FileSystem.AppDataDirectory, "token.json");

        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.FromStream(stream).Secrets,
            new[] { DriveService.Scope.DriveFile },
            "user",
            CancellationToken.None,
            new FileDataStore(credPath, true));

        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        // Step 1: Locate or create the MeetMind folder
        var fileList = await service.Files.List().ExecuteAsync();
        var folder = fileList.Files.FirstOrDefault(f => f.Name == MeetMindFolderName && f.MimeType == "application/vnd.google-apps.folder");

        if (folder == null)
        {
            var folderMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = MeetMindFolderName,
                MimeType = "application/vnd.google-apps.folder"
            };
            var folderRequest = service.Files.Create(folderMetadata);
            folderRequest.Fields = "id";
            folder = await folderRequest.ExecuteAsync();
        }

        // Step 2: Upload file to that folder
        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = Path.GetFileName(zipPath),
            Parents = new[] { folder.Id }
        };

        using var fileStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read);
        var request = service.Files.Create(fileMetadata, fileStream, "application/zip");
        request.Fields = "id";
        await request.UploadAsync();
    }
}