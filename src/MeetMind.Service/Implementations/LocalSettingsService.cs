using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MeetMind.Service.Contracts;
using MeetMind.Service.Models;

namespace MeetMind.Service.Implementations;

public class LocalSettingsService : ISettingsService
{
    private const string FileName = "settings.json";

    public async Task<UserSettings> LoadAsync()
    {
        try
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, FileName);
            if (!File.Exists(path))
                return new UserSettings();

            var json = await File.ReadAllTextAsync(path);
            var settings = JsonSerializer.Deserialize<UserSettings>(json);
            return settings ?? new UserSettings();
        }
        catch
        {
            return new UserSettings();
        }
    }

    public async Task SaveAsync(UserSettings settings)
    {
        try
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, FileName);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] Failed to save settings: " + ex.Message);
        }
    }
}