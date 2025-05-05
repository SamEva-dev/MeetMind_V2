using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MeetMind.Service.Contracts;
using MeetMind.Service.Models;

namespace MeetMind.Service.Implementations;

public class JsonVoiceMappingStore : IVoiceMappingStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public JsonVoiceMappingStore()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "voice_mappings.json");
    }

    public async Task SaveMappingAsync(string voiceId, string name)
    {
        await _semaphore.WaitAsync();
        try
        {
            var list = await LoadAsync();
            var existing = list.FirstOrDefault(v => v.Id == voiceId);
            if (existing != null)
            {
                existing.Name = name;
            }
            else
            {
                list.Add(new VoicePrint { Id = voiceId, Name = name });
            }
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<string?> GetMappedNameAsync(string voiceId)
    {
        var list = await LoadAsync();
        return list.FirstOrDefault(v => v.Id == voiceId)?.Name;
    }

    public async Task<IList<VoicePrint>> GetAllAsync()
    {
        return await LoadAsync();
    }

    private async Task<List<VoicePrint>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<VoicePrint>();

        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<VoicePrint>>(json) ?? new List<VoicePrint>();
    }
}