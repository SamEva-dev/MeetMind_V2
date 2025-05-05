using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeetMind.Service.Models;

namespace MeetMind.Service.Contracts;

public interface IVoiceMappingStore
{
    Task SaveMappingAsync(string voiceId, string name);
    Task<string?> GetMappedNameAsync(string voiceId);
    Task<IList<VoicePrint>> GetAllAsync();
}